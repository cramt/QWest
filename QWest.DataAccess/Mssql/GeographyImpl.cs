using Model.Geographic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;

namespace QWest.DataAcess.Mssql {
    class GeographyImpl : IGeography {
        private SqlConnection _conn;
        public GeographyImpl(SqlConnection conn) {
            _conn = conn;
        }
        public async Task InsertBackup(IEnumerable<Country> countries) {
            int i = 0;
            Func<string, Subdivision, List<(string name, object parameter)>, StringBuilder, string> recSubdivisionInsert = null;
            recSubdivisionInsert = (id, subdivision, preparedQueryParams, declarations) => {
                i++;
                preparedQueryParams.Add(("@alpha_2" + i, subdivision.Alpha2));
                preparedQueryParams.Add(("@name" + i, subdivision.Name));
                preparedQueryParams.Add(("@type" + i, subdivision.Type));
                string q = "" +
                "INSERT INTO geopolitical_location " +
                "(alpha_2, name, type, super_id) " +
                "VALUES " +
                $"(CAST(@alpha_2{i} as CHAR(2)), @name{i}, @type{i}, {id}); ";
                if (subdivision.Subdivisions.Count() != 0) {
                    declarations.Append($"@last_id{i} INT, ");
                    string thisid = "@last_id" + i;
                    q += "" +
                    $"SET @last_id{i} = CAST(scope_identity() as int); " +
                    string.Join("", subdivision.Subdivisions.Select(x => recSubdivisionInsert(thisid, x, preparedQueryParams, declarations)).ToArray());
                }
                return q;
            };
            IEnumerable<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> rawQueries = countries.Select((country) => {
                i++;
                StringBuilder declarations = new StringBuilder();
                declarations.Append($"@last_id{i} INT, ");
                List<(string name, object parameter)> preparedQueryParams = new List<(string name, object parameter)>();
                preparedQueryParams.Add(("@alpha_2" + i, country.Alpha2.ToCharArray()));
                preparedQueryParams.Add(("@alpha_3" + i, country.Alpha3.ToCharArray()));
                preparedQueryParams.Add(("@name" + i, country.Name));
                preparedQueryParams.Add(("@official_name" + i, country.OfficialName ?? SqlString.Null));
                preparedQueryParams.Add(("@common_name" + i, country.CommonName ?? SqlString.Null));
                preparedQueryParams.Add(("@type" + i, country.Type));
                preparedQueryParams.Add(("@numeric" + i, country.Numeric));
                string thisid = "@last_id" + i;
                string query = "" +
                "INSERT INTO geopolitical_location " +
                "(alpha_2, alpha_3, name, official_name, common_name, type, numeric) " +
                "VALUES " +
                $"(CAST(@alpha_2{i} as CHAR(2)), @alpha_3{i}, @name{i}, @official_name{i}, @common_name{i}, @type{i}, @numeric{i}); " +
                $"SET @last_id{i} = CAST(scope_identity() as int); " +
                string.Join("", country.Subdivisions.Select(x => recSubdivisionInsert(thisid, x, preparedQueryParams, declarations)).ToArray());
                return (preparedQueryParams, declarations.ToString(), query);
            });
            IEnumerable<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> queries = rawQueries.Aggregate(new List<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> { (new List<(string name, object parameter)>(), "", "") }, (acc, x) => {
                int totalAmount = acc[acc.Count - 1].preparedQueryParams.Count + x.preparedQueryParams.Count;
                if (totalAmount > 2000) {
                    acc.Add(x);
                }
                else {
                    acc[acc.Count - 1] = (acc[acc.Count - 1].preparedQueryParams.Concat(x.preparedQueryParams).ToList(), acc[acc.Count - 1].declarations + x.declarations, acc[acc.Count - 1].query + x.query);
                }
                return acc;
            });
            await Task.WhenAll(queries.Select(x => {
                SqlCommand stmt = _conn.CreateCommand();
                stmt.CommandText = "DECLARE " + x.declarations.Substring(0, x.declarations.Length - 2) + ";" + x.query;
                foreach ((string name, object parameter) in x.preparedQueryParams) {
                    stmt.Parameters.AddWithValue(name, parameter);
                }
                return stmt.ExecuteNonQueryAsync();
            }).ToArray());
        }
        public async Task<IEnumerable<Country>> CreateBackup() {
            IEnumerable<GeopoliticalLocationDbRep> locals = (await _conn.CreateCommand("SELECT id, alpha_2, alpha_3, name, official_name, common_name, type, numeric, super_id FROM geopolitical_location")
                .ExecuteReaderAsync())
                .ToIterator(x => new GeopoliticalLocationDbRep(x));
            return GeopoliticalLocationDbRep.ToTreeStructure(locals).Cast<Country>();


        }

        public async Task<Country> GetCountryByAlpha2(string alpha2) {
            SqlCommand stmt = _conn.CreateCommand(@"
DECLARE @curr TABLE(g_id INT NOT NULL);
DECLARE @temp TABLE(g_id INT NOT NULL);
DECLARE @result TABLE(g_id INT NOT NULL);

INSERT INTO @curr SELECT id FROM geopolitical_location WHERE alpha_2 = @alpha_2 AND type = 'Country';


WHILE (SELECT COUNT(*) FROM @curr) != 0
BEGIN
	DELETE FROM @temp;
	INSERT INTO @temp SELECT sub.id FROM geopolitical_location sub INNER JOIN geopolitical_location super ON sub.super_id = super.id INNER JOIN @curr c ON super.id = c.g_id
	INSERT INTO @result SELECT g_id FROM @curr;
	DELETE FROM @curr;
	INSERT INTO @curr SELECT g_id FROM @temp;
END

SELECT id, alpha_2, alpha_3, name, official_name, common_name, type, numeric, super_id FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
");
            stmt.Parameters.AddWithValue("@alpha_2", alpha2);
            return (Country)GeopoliticalLocationDbRep.ToTreeStructure((await stmt.ExecuteReaderAsync()).ToIterator(x => new GeopoliticalLocationDbRep(x))).FirstOrDefault();
        }

        public Task<GeopoliticalLocation> GetAnyByAlpha2s(string alphas2) {
            return GetAnyByAlpha2s(alphas2.Split('-'));
        }

        public async Task<GeopoliticalLocation> GetAnyByAlpha2s(IEnumerable<string> alpha2s) {
            string query = alpha2s.Aggregate((q: "", i: 0), (acc, x) => {
                string q = acc.q;
                int i = acc.i;
                if (i == 0) {
                    q = $"SELECT id FROM geopolitical_location a{i} WHERE a{i}.alpha_2 = @alpha_2_{i} AND super_id IS NULL";
                }
                else {
                    q = $"SELECT id FROM geopolitical_location a{i} WHERE a{i}.alpha_2 = @alpha_2_{i} AND super_id = ({q})";
                }
                return (q, i + 1);
            }).q;
            query = $@"
DECLARE @curr TABLE(g_id INT NOT NULL);
DECLARE @temp TABLE(g_id INT NOT NULL);
DECLARE @result TABLE(g_id INT NOT NULL);

INSERT INTO @curr {query};


WHILE (SELECT COUNT(*) FROM @curr) != 0
BEGIN
	DELETE FROM @temp;
	INSERT INTO @temp SELECT sub.id FROM geopolitical_location sub INNER JOIN geopolitical_location super ON sub.super_id = super.id INNER JOIN @curr c ON super.id = c.g_id
	INSERT INTO @result SELECT g_id FROM @curr;
	DELETE FROM @curr;
	INSERT INTO @curr SELECT g_id FROM @temp;
END

SELECT id, alpha_2, alpha_3, name, official_name, common_name, type, numeric, super_id FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
";
            SqlCommand stmt = _conn.CreateCommand(query);
            int j = 0;
            foreach (string alpha2 in alpha2s) {
                stmt.Parameters.AddWithValue("@alpha_2_" + j, alpha2);
                j++;
            }
            return GeopoliticalLocationDbRep.ToTreeStructure((await stmt.ExecuteReaderAsync())
                .ToIterator(x => new GeopoliticalLocationDbRep(x))).FirstOrDefault();
        }
    }
}
