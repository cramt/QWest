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

namespace QWest.DataAcess {
    public static partial class DAO {
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
            [JsonProperty("official_name")]
            public string OfficialName { get; }
            [JsonProperty("common_name")]
            public string CommonName { get; }
            [JsonProperty("type")]
            public string Type { get; }
            [JsonProperty("numeric")]
            public int? Numeric { get; }
            [JsonProperty("super_id")]
            public int? SuperId { get; }

            public GeopoliticalLocationDbRep(SqlDataReader reader) {
                Id = reader.GetSqlInt32(0).Value;
                Alpha2 = reader.GetSqlString(1).Value;
                Alpha3 = reader.GetSqlString(2).NullableValue();
                Name = reader.GetSqlString(3).Value;
                Names = JsonConvert.DeserializeObject<List<string>>(reader.GetSqlString(4).Value);
                OfficialName = reader.GetSqlString(5).NullableValue();
                CommonName = reader.GetSqlString(6).NullableValue();
                Type = reader.GetSqlString(7).Value;
                Numeric = reader.GetSqlInt32(8).NullableValue();
                SuperId = reader.GetSqlInt32(9).NullableValue();
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
        public interface IGeography {
            Task InsertBackup(IEnumerable<Country> countries);
            Task<IEnumerable<Country>> CreateBackup();
            Task<Country> GetCountryByAlpha2(string alpha2);
            Task<GeopoliticalLocation> GetAnyByAlpha2s(string alphas2);
            Task<GeopoliticalLocation> GetAnyByAlpha2s(IEnumerable<string> alpha2s);
            Task Update(GeopoliticalLocation location);
            Task Delete(GeopoliticalLocation location);
            Task Delete(int id);
            Task<int> Add(GeopoliticalLocation location);
        }

        public static IGeography Geography { get; set; } = new Mssql.GeographyImpl(ConnectionWrapper.Instance);
    }
}
