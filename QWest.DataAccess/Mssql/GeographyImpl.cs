using Model.Geographic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;

namespace QWest.DataAcess.Mssql {
    class GeographyImpl : IGeography {
        [Serializable]
        internal class GeopoliticalLocationDbRep : IDbRep<GeopoliticalLocation> {
            [JsonProperty("id")]
            public int Id { get; }
            [JsonProperty("alpha_2")]
            public string Alpha2 { get; }
            [JsonProperty("alpha_3")]
            public string Alpha3 { get; }
            [JsonProperty("name")]
            public string Name { get; }
            [JsonProperty("names")]
            public List<string> Names { get; }
            [JsonProperty("region")]
            public string Region { get; }
            [JsonProperty("sub_region")]
            public string SubRegion { get; }
            [JsonProperty("intermediate_region")]
            public string IntermediateRegion { get; }
            [JsonProperty("region_code")]
            public int? RegionCode { get; }
            [JsonProperty("sub_region_code")]
            public int? SubRegionCode { get; }
            [JsonProperty("intermediate_region_code")]
            public int? IntermediateRegionCode { get; }

            public int? SuperId { get; }

            public GeopoliticalLocationDbRep(SqlDataReader reader) {
                Id = reader.GetSqlInt32(0).Value;
                Alpha2 = reader.GetSqlString(1).Value;
                Alpha3 = reader.GetSqlString(2).NullableValue();
                Name = reader.GetSqlString(3).Value;
                Names = JsonConvert.DeserializeObject<List<string>>(reader.GetSqlString(4).Value);
                SuperId = reader.GetSqlInt32(5).NullableValue();
                Region = reader.GetSqlString(6).NullableValue();
                SubRegion = reader.GetSqlString(7).NullableValue();
                IntermediateRegion = reader.GetSqlString(8).NullableValue();
                RegionCode = reader.GetSqlInt32(9).NullableValue();
                SubRegionCode = reader.GetSqlInt32(10).NullableValue();
                IntermediateRegionCode = reader.GetSqlInt32(11).NullableValue();
            }

            public static IEnumerable<GeopoliticalLocationDbRep> FromJson(string json) {
                return JsonConvert.DeserializeObject<IEnumerable<GeopoliticalLocationDbRep>>(json);
            }

            public static GeopoliticalLocationDbRep FromJsonSingle(string json) {
                return JsonConvert.DeserializeObject<GeopoliticalLocationDbRep>(json);
            }

            public bool IsCountry {
                get {
                    return SuperId == null;
                }
            }

            public GeopoliticalLocation ToModel() {
                if (IsCountry) {
                    return new Country {
                        Id = Id,
                        Alpha2 = Alpha2,
                        Name = Name,
                        Alpha3 = Alpha3,
                        Names = Names,
                        Region = Region,
                        SubRegion = SubRegion,
                        IntermediateRegion = IntermediateRegion,
                        RegionCode = RegionCode,
                        SubRegionCode = SubRegionCode,
                        IntermediateRegionCode = IntermediateRegionCode,
                        Subdivisions = new List<Subdivision>()
                    };
                }
                else {
                    return new Subdivision {
                        Alpha2 = Alpha2,
                        Name = Name,
                        Subdivisions = new List<Subdivision>(),
                        Id = Id,
                        SuperId = (int)SuperId
                    };
                }
            }

            public static IEnumerable<GeopoliticalLocation> ToTreeStructure(IEnumerable<GeopoliticalLocationDbRep> locations) {
                IEnumerable<GeopoliticalLocation> entities = locations.Select(x => x.ToModel()).ToList();
                Dictionary<int, GeopoliticalLocation> map = entities.ToDictionary(x => (int)x.Id, y => y);
                foreach (GeopoliticalLocation local in map.Values) {
                    if (local is Subdivision sub && map.ContainsKey(sub.SuperId)) {
                        GeopoliticalLocation parent = map[sub.SuperId];
                        sub.Parent = parent;
                        parent.Subdivisions.Add(sub);
                    }
                }
                return map.Values.Where(x => {
                    if (x is Subdivision sub) {
                        return !map.ContainsKey(sub.SuperId);
                    }
                    return true;
                });
            }
        }
        private ConnectionWrapper _conn;
        public GeographyImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public Task InsertBackup(IEnumerable<Country> countries) {
            return InsertBackup(countries, _conn.Connection);
        }
        public async Task InsertBackup(IEnumerable<Country> countries, SqlConnection conn) {
            int i = 0;
            Func<string, Subdivision, List<(string name, object parameter)>, StringBuilder, string> recSubdivisionInsert = null;
            recSubdivisionInsert = (id, subdivision, preparedQueryParams, declarations) => {
                i++;
                preparedQueryParams.Add(("@alpha_2" + i, subdivision.Alpha2));
                preparedQueryParams.Add(("@name" + i, subdivision.Name));
                preparedQueryParams.Add(("@names" + i, JsonConvert.SerializeObject(subdivision.Names ?? new List<string>())));
                string q = $@"
INSERT INTO geopolitical_location
(alpha_2, name, names, super_id)
VALUES
(CAST(@alpha_2{i} as CHAR(2)), @name{i}, @names{i}, {id}); 
";
                if (subdivision.Subdivisions.Count() != 0) {
                    declarations.Append($"@last_id{i} INT, ");
                    string thisid = "@last_id" + i;
                    q += $"SET @last_id{i} = CAST(scope_identity() as int); " +
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
                preparedQueryParams.Add(("@region" + i, country.Region ?? SqlString.Null));
                preparedQueryParams.Add(("@sub_region" + i, country.SubRegion ?? SqlString.Null));
                preparedQueryParams.Add(("@intermediate_region" + i, country.IntermediateRegion ?? SqlString.Null));
                preparedQueryParams.Add(("@region_code" + i, country.RegionCode ?? SqlInt32.Null));
                preparedQueryParams.Add(("@sub_region_code" + i, country.SubRegionCode ?? SqlInt32.Null));
                preparedQueryParams.Add(("@intermediate_region_code" + i, country.IntermediateRegionCode ?? SqlInt32.Null));
                string thisid = "@last_id" + i;
                string query = $@"
INSERT INTO geopolitical_location
(alpha_2, alpha_3, name, names, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code)
VALUES
(CAST(@alpha_2{i} as CHAR(2)), @alpha_3{i}, @name{i}, @names{i}, @region{i}, @sub_region{i}, @intermediate_region{i}, @region_code{i}, @sub_region_code{i}, @intermediate_region_code{i}); 
SET @last_id{i} = CAST(scope_identity() as int); 
" +
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

            if (conn.State == ConnectionState.Closed) {
                conn.Open();
            }
            foreach ((List<(string name, object parameter)> preparedQueryParams, string declarations, string query) query in queries) {
                SqlCommand stmt = conn.CreateCommand();
                stmt.CommandText = "DECLARE " + query.declarations.Substring(0, query.declarations.Length - 2) + ";" + query.query;
                foreach ((string name, object parameter) in query.preparedQueryParams) {
                    stmt.Parameters.AddWithValue(name, parameter);
                }
                await stmt.ExecuteNonQueryAsync();
            }
            conn.Close();
        }
        public async Task<IEnumerable<Country>> CreateBackup() {
            ;
            IEnumerable<GeopoliticalLocationDbRep> locals = await _conn.Use("SELECT id, alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code FROM geopolitical_location",
                async stmt => (await stmt.ExecuteReaderAsync())
                .ToIterator(x => new GeopoliticalLocationDbRep(x)).ToList());
            return GeopoliticalLocationDbRep.ToTreeStructure(locals).Cast<Country>();


        }

        public Task<Country> GetCountryByAlpha2(string alpha2) {
            return _conn.Use(@"
DECLARE @curr TABLE(g_id INT NOT NULL);
DECLARE @temp TABLE(g_id INT NOT NULL);
DECLARE @result TABLE(g_id INT NOT NULL);

INSERT INTO @curr SELECT id FROM geopolitical_location WHERE alpha_2 = @alpha_2 AND super_id IS NULL;


WHILE (SELECT COUNT(*) FROM @curr) != 0
BEGIN
	DELETE FROM @temp;
	INSERT INTO @temp SELECT sub.id FROM geopolitical_location sub INNER JOIN geopolitical_location super ON sub.super_id = super.id INNER JOIN @curr c ON super.id = c.g_id
	INSERT INTO @result SELECT g_id FROM @curr;
	DELETE FROM @curr;
	INSERT INTO @curr SELECT g_id FROM @temp;
END

SELECT id, alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
", async stmt => {
                stmt.Parameters.AddWithValue("@alpha_2", alpha2);
                return (Country)GeopoliticalLocationDbRep.ToTreeStructure((await stmt.ExecuteReaderAsync()).ToIterator(x => new GeopoliticalLocationDbRep(x))).FirstOrDefault();
            });
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

SELECT id, alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code FROM geopolitical_location INNER JOIN @result r ON geopolitical_location.id = r.g_id;
";

            return GeopoliticalLocationDbRep.ToTreeStructure(await _conn.Use(query, async stmt => {
                int j = 0;
                foreach (string alpha2 in alpha2s) {
                    stmt.Parameters.AddWithValue("@alpha_2_" + j, alpha2);
                    j++;
                }
                return (await stmt.ExecuteReaderAsync())
                .ToIterator(x => new GeopoliticalLocationDbRep(x));
            })).FirstOrDefault();
        }

        public async Task Update(GeopoliticalLocation location) {
            string query = @"
UPDATE geopolitical_location
SET
alpha_2 = @alpha_2, 
alpha_3 = @alpha_3,
name = @name, 
names = @names, 
super_id = @super_id, 
region = @region, 
sub_region = @sub_region, 
intermediate_region = @intermediate_region, 
region_code = @region_code, 
sub_region_code = @sub_region_code, 
intermediate_region_code = @intermediate_region_code
WHERE
id = @id
";
            await _conn.Use(query, stmt => {
                if (location is Country country) {
                    stmt.Parameters.AddWithValue("@alpha_2", country.Alpha2);
                    stmt.Parameters.AddWithValue("@alpha_3", country.Alpha3);
                    stmt.Parameters.AddWithValue("@name", country.Name);
                    stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(country.Names ?? new List<string>()));
                    stmt.Parameters.AddWithValue("@region", country.Region ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@sub_region", country.SubRegion ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region", country.IntermediateRegion ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@region_code", country.RegionCode ?? SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@sub_region_code", country.SubRegionCode ?? SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region_code", country.IntermediateRegionCode ?? SqlInt32.Null);

                    stmt.Parameters.AddWithValue("@super_id", SqlInt32.Null);
                }
                else if (location is Subdivision subdivision) {
                    stmt.Parameters.AddWithValue("@alpha_2", subdivision.Alpha2);
                    stmt.Parameters.AddWithValue("@name", subdivision.Name);
                    stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(subdivision.Names ?? new List<string>()));
                    stmt.Parameters.AddWithValue("@super_id", subdivision.SuperId);

                    stmt.Parameters.AddWithValue("@alpha_3", SqlString.Null);
                    stmt.Parameters.AddWithValue("@region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@sub_region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@region_code", SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@sub_region_code", SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region_code", SqlInt32.Null);
                }
                else {
                    throw new ArgumentException("supplied a geopolitical location that was neither country or subdivision");
                }

                return stmt.ExecuteNonQueryAsync();
            });
        }

        public Task Delete(GeopoliticalLocation location) {
            return Delete(location.Id ?? 0);
        }

        public async Task Delete(int id) {
            string query = @"
DELETE FROM 
geopolitical_location
WHERE
id = @id
";
            await _conn.Use(query, stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return stmt.ExecuteNonQueryAsync();
            });
        }

        public async Task<int> Add(GeopoliticalLocation location) {
            string query = @"
INSERT INTO geopolitical_location
(alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code)
VALUES
(@alpha_2, @alpha_3, @name, @names, @super_id, @region, @sub_region, @intermediate_region, @region_code, @sub_region_code, @intermediate_region_code);
SELECT CAST(scope_identity() AS int)
";
            int id = await _conn.Use(query, async stmt => {
                if (location is Country country) {
                    stmt.Parameters.AddWithValue("@alpha_2", country.Alpha2);
                    stmt.Parameters.AddWithValue("@alpha_3", country.Alpha3);
                    stmt.Parameters.AddWithValue("@name", country.Name);
                    stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(country.Names ?? new List<string>()));
                    stmt.Parameters.AddWithValue("@region", country.Region ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@sub_region", country.SubRegion ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region", country.IntermediateRegion ?? SqlString.Null);
                    stmt.Parameters.AddWithValue("@region_code", country.RegionCode ?? SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@sub_region_code", country.SubRegionCode ?? SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region_code", country.IntermediateRegionCode ?? SqlInt32.Null);

                    stmt.Parameters.AddWithValue("@super_id", SqlInt32.Null);
                }
                else if (location is Subdivision subdivision) {
                    stmt.Parameters.AddWithValue("@alpha_2", subdivision.Alpha2);
                    stmt.Parameters.AddWithValue("@name", subdivision.Name);
                    stmt.Parameters.AddWithValue("@names", JsonConvert.SerializeObject(subdivision.Names ?? new List<string>()));
                    stmt.Parameters.AddWithValue("@super_id", subdivision.SuperId);

                    stmt.Parameters.AddWithValue("@alpha_3", SqlString.Null);
                    stmt.Parameters.AddWithValue("@region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@sub_region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region", SqlString.Null);
                    stmt.Parameters.AddWithValue("@region_code", SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@sub_region_code", SqlInt32.Null);
                    stmt.Parameters.AddWithValue("@intermediate_region_code", SqlInt32.Null);
                }
                else {
                    throw new ArgumentException("supplied a geopolitical location that was neither country or subdivision");
                }
                return (int)await stmt.ExecuteScalarAsync();
            });
            location.Id = id;
            return id;
        }
        public Task<IEnumerable<Country>> GetCountries() {
            string query = @"
SELECT
id, alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code
FROM
geopolitical_location
WHERE
super_id IS NULL
";
            return _conn.Use(query, async stmt => (await stmt.ExecuteReaderAsync()).ToIterator(x => (Country)new GeopoliticalLocationDbRep(x).ToModel()));
        }
        public async Task<IEnumerable<Subdivision>> GetSubdivisions(GeopoliticalLocation location) {
            var results = (await GetSubdivisions((int)location.Id)).ToList();
            location.Subdivisions = results;
            return results;
        }
        public async Task<IEnumerable<Subdivision>> GetSubdivisions(int superId) {
            string query = @"
SELECT
id, alpha_2, alpha_3, name, names, super_id, region, sub_region, intermediate_region, region_code, sub_region_code, intermediate_region_code
FROM
geopolitical_location
WHERE
super_id = @super_id
";
            return await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@super_id", superId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => (Subdivision)new GeopoliticalLocationDbRep(x).ToModel()).ToList();
            });
        }
    }
}
