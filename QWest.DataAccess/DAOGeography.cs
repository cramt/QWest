using Model.Geographic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class Geography {
            private class GeopoliticalLocationDbRep {
                public int Id { get; }
                public string Alpha2 { get; }
                public string Alpha3 { get; }
                public string Name { get; }
                public string OfficialName { get; }
                public string CommonName { get; }
                public string Type { get; }
                public int? Numeric { get; }
                public int? SuperId { get; }

                public GeopoliticalLocationDbRep(SqlDataReader reader) {
                    Id = reader.GetSqlInt32(0).Value;
                    Alpha2 = reader.GetSqlString(1).Value;
                    Alpha3 = reader.GetSqlString(2).NullableValue();
                    Name = reader.GetSqlString(3).Value;
                    OfficialName = reader.GetSqlString(4).NullableValue();
                    CommonName = reader.GetSqlString(5).NullableValue();
                    Type = reader.GetSqlString(6).Value;
                    Numeric = reader.GetSqlInt32(7).NullableValue();
                    SuperId = reader.GetSqlInt32(8).NullableValue();
                }

                public bool IsCountry {
                    get {
                        return SuperId == null;
                    }
                }

                public GeopoliticalLocation ToModel() {
                    if (IsCountry) {
                        return new Country {
                            Alpha2 = Alpha2,
                            Name = Name,
                            Alpha3 = Alpha3,
                            OfficialName = OfficialName,
                            CommonName = CommonName,
                            Numeric = (int)Numeric,
                            Subdivisions = new List<Subdivision>(),
                            Id = Id,
                        };
                    }
                    else {
                        return new Subdivision {
                            Alpha2 = Alpha2,
                            Name = Name,
                            Type = Type,
                            Subdivisions = new List<Subdivision>(),
                            Id = Id,
                            SuperId = (int)SuperId
                        };
                    }
                }

                public static IEnumerable<Country> ToTreeStructure(IEnumerable<GeopoliticalLocationDbRep> locations) {
                    IEnumerable<GeopoliticalLocation> entities = locations.Select(x => x.ToModel()).ToList();
                    Dictionary<int, GeopoliticalLocation> map = entities.ToDictionary(x => x.Id, y => y);
                    foreach (Subdivision sub in map.Values.Where(x => x is Subdivision).Select(x => (Subdivision)x)) {
                        GeopoliticalLocation parent = map[sub.SuperId];
                        sub.Parent = parent;
                        parent.Subdivisions.Add(sub);
                    }
                    return map.Values.Where(x => x is Country).Select(x => (Country)x);
                }
            }
            public static Task InsertBackup(IEnumerable<Country> countries) {
                return InsertBackup(countries, ConnectionWrapper.Instance);
            }
            public static async Task InsertBackup(IEnumerable<Country> countries, SqlConnection conn) {
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
                    SqlCommand stmt = conn.CreateCommand();
                    stmt.CommandText = "DECLARE " + x.declarations.Substring(0, x.declarations.Length - 2) + ";" + x.query;
                    foreach ((string name, object parameter) in x.preparedQueryParams) {
                        stmt.Parameters.AddWithValue(name, parameter);
                    }
                    return stmt.ExecuteNonQueryAsync();
                }).ToArray());
            }
            public static async Task<IEnumerable<Country>> CreateBackup() {
                IEnumerable<GeopoliticalLocationDbRep> locals = (await ConnectionWrapper.CreateCommand("SELECT id, alpha_2, alpha_3, name, official_name, common_name, type, numeric, super_id FROM geopolitical_location")
                    .ExecuteReaderAsync())
                    .ToIterator(x => new GeopoliticalLocationDbRep(x));
                return GeopoliticalLocationDbRep.ToTreeStructure(locals);


            }

            public static async Task<Country> GetCountryByAlpha2(string alpha2) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand(@"
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
                return GeopoliticalLocationDbRep.ToTreeStructure((await stmt.ExecuteReaderAsync()).ToIterator(x => new GeopoliticalLocationDbRep(x))).FirstOrDefault();
            }
        }
    }
}
