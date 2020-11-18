using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Geographic {
    [Serializable]
    public abstract class GeopoliticalLocation {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("alpha_2")]
        public string Alpha2 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("names")]
        public List<string> Names { get; set; }

        [JsonProperty("subdivisions")]
        public List<Subdivision> Subdivisions { get; set; }

        public static List<Country> Parse(string json) {
            List<Country> countries = JsonConvert.DeserializeObject<List<Country>>(json);
            Action<GeopoliticalLocation, Subdivision> traverse = null;
            traverse = (parent, curr) => {
                curr.Parent = parent;
                curr.Subdivisions.ForEach(x => traverse(curr, x));
            };
            countries.ForEach(x => x.Subdivisions.ForEach(y => traverse(x, y)));
            return countries;
        }

        public static IEnumerable<GeopoliticalLocation> Traverse(IEnumerable<Country> countries) {
            return countries.Select(Traverse).SelectMany(x => x);
        }

        public static IEnumerable<GeopoliticalLocation> Traverse(GeopoliticalLocation location) {
            return location.Subdivisions.Select(Traverse).SelectMany(x => x).Concat(new GeopoliticalLocation[] { location });
        }
    }
}
