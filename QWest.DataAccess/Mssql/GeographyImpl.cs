using Model.Geographic;
using Newtonsoft.Json;
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
                preparedQueryParams.Add(("@names" + i, JsonConvert.SerializeObject(subdivision.Names ?? new List<string>())));
                preparedQueryParams.Add(("@type" + i, subdivision.Type));
                string q = "" +
                "INSERT INTO geopolitical_location " +
                "(alpha_2, name, names, type, super_id) " +
                "VALUES " +
                $"(CAST(@alpha_2{i} as CHAR(2)), @name{i}, @names{i}, @type{i}, {id}); ";
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
                preparedQueryParams.Add(("@names" + i, JsonConvert.SerializeObject(country.Names ?? new List<string>())));
                preparedQueryParams.Add(("@official_name" + i, country.OfficialName ?? SqlString.Null));
                preparedQueryParams.Add(("@common_name" + i, country.CommonName ?? SqlString.Null));
                preparedQueryParams.Add(("@type" + i, country.Type));
                preparedQueryParams.Add(("@numeric" + i, country.Numeric));
                string thisid = "@last_id" + i;
                string query = "" +
                "INSERT INTO geopolitical_location " +
                "(alpha_2, alpha_3, name, names, official_name, common_name, type, numeric) " +
                "VALUES " +
                $"(CAST(@alpha_2{i} as CHAR(2)), @alpha_3{i}, @name{i}, @names{i}, @official_name{i}, @common_name{i}, @type{i}, @numeric{i}); " +
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
            IEnumerable<GeopoliticalLocationDbRep> locals = (await _conn.CreateCommand("SELECT id, alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id FROM geopolitical_location")
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

SELECT id, alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
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

SELECT id, alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
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

        public async Task Update(GeopoliticalLocation location) {
            SqlCommand stmt = _conn.CreateCommand(@"
UPDATE geopolitical_location
SET
alpha_2 = @alpha_2,
alpha_3 = @alpha_3,
name = @name,
names = @names,
offical_name = @offical_name,
common_name = @common_name,
type = @type,
numeric = @numeric,
super_id = @super_id
WHERE
id = @id
");
            if (location is Country country) {
                stmt.Parameters.AddWithValue("@alpha_2", country.Alpha2);
                stmt.Parameters.AddWithValue("@alpha_3", country.Alpha3);
                stmt.Parameters.AddWithValue("@name", country.Name);
                stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(country.Names ?? new List<string>()));
                stmt.Parameters.AddWithValue("@offical_name", country.OfficialName ?? SqlString.Null);
                stmt.Parameters.AddWithValue("@common_name", country.CommonName ?? SqlString.Null);
                stmt.Parameters.AddWithValue("@type", country.Type);
                stmt.Parameters.AddWithValue("@numeric", country.Numeric);

                stmt.Parameters.AddWithValue("@super_id", SqlInt32.Null);
            }
            else if (location is Subdivision subdivision) {
                stmt.Parameters.AddWithValue("@alpha_2", subdivision.Alpha2);
                stmt.Parameters.AddWithValue("@name", subdivision.Name);
                stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(subdivision.Names ?? new List<string>()));
                stmt.Parameters.AddWithValue("@type", subdivision.Type);
                stmt.Parameters.AddWithValue("@super_id", subdivision.SuperId);

                stmt.Parameters.AddWithValue("@alpha_3", SqlString.Null);
                stmt.Parameters.AddWithValue("@offical_name", SqlString.Null);
                stmt.Parameters.AddWithValue("@common_name", SqlString.Null);
                stmt.Parameters.AddWithValue("@numeric", SqlInt32.Null);
            }
            else {
                throw new ArgumentException("supplied a geopolitical location that was neither country or subdivision");
            }

            await stmt.ExecuteNonQueryAsync();
        }

        public Task Delete(GeopoliticalLocation location) {
            return Delete(location.Id ?? 0);
        }

        public async Task Delete(int id) {
            SqlCommand stmt = _conn.CreateCommand(@"
DELETE FROM 
geopolitical_location
WHERE
id = @id
");
            stmt.Parameters.AddWithValue("@id", id);
            await stmt.ExecuteNonQueryAsync();
        }

        public async Task<int> Add(GeopoliticalLocation location) {
            SqlCommand stmt = _conn.CreateCommand(@"
INSERT INTO geopolitical_location
(alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id)
VALUES
(@alpha_2, @alpha_3, @name, @names, @official_name, @common_name, @type, @numeric, @super_id);
SELECT CAST(scope_identity() AS int)
");
            if (location is Country country) {
                stmt.Parameters.AddWithValue("@alpha_2", country.Alpha2);
                stmt.Parameters.AddWithValue("@alpha_3", country.Alpha3);
                stmt.Parameters.AddWithValue("@name", country.Name);
                stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(country.Names ?? new List<string>()));
                stmt.Parameters.AddWithValue("@offical_name", country.OfficialName ?? SqlString.Null);
                stmt.Parameters.AddWithValue("@common_name", country.CommonName ?? SqlString.Null);
                stmt.Parameters.AddWithValue("@type", country.Type);
                stmt.Parameters.AddWithValue("@numeric", country.Numeric);

                stmt.Parameters.AddWithValue("@super_id", SqlInt32.Null);
            }
            else if (location is Subdivision subdivision) {
                stmt.Parameters.AddWithValue("@alpha_2", subdivision.Alpha2);
                stmt.Parameters.AddWithValue("@name", subdivision.Name);
                stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(subdivision.Names ?? new List<string>()));
                stmt.Parameters.AddWithValue("@type", subdivision.Type);
                stmt.Parameters.AddWithValue("@super_id", subdivision.SuperId);

                stmt.Parameters.AddWithValue("@alpha_3", SqlString.Null);
                stmt.Parameters.AddWithValue("@offical_name", SqlString.Null);
                stmt.Parameters.AddWithValue("@common_name", SqlString.Null);
                stmt.Parameters.AddWithValue("@numeric", SqlInt32.Null);
            }
            else {
                throw new ArgumentException("supplied a geopolitical location that was neither country or subdivision");
            }
            int id = (int)await stmt.ExecuteScalarAsync();
            location.Id = id;
            return id;
        }
        public async Task<IEnumerable<Country>> GetCountries() {
            SqlCommand stmt = _conn.CreateCommand(@"
SELECT
id, alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id
FROM
geopolitical_location
WHERE
super_id IS NULL
");
            return (await stmt.ExecuteReaderAsync()).ToIterator(x => (Country)new GeopoliticalLocationDbRep(x).ToModel());
        }
        public async Task<IEnumerable<Subdivision>> GetSubdivisions(GeopoliticalLocation location) {
            var results = (await GetSubdivisions((int)location.Id)).ToList();
            location.Subdivisions = results;
            return results;
        }
        public async Task<IEnumerable<Subdivision>> GetSubdivisions(int superId) {
            SqlCommand stmt = _conn.CreateCommand(@"
SELECT
id, alpha_2, alpha_3, name, names, official_name, common_name, type, numeric, super_id
FROM
geopolitical_location
WHERE
super_id = @super_id
");
            stmt.Parameters.AddWithValue("@super_id", superId);
            return (await stmt.ExecuteReaderAsync()).ToIterator(x => (Subdivision)new GeopoliticalLocationDbRep(x).ToModel()).ToList();
        }
    }
}
