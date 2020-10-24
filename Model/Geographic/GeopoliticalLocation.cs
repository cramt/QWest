using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Model.Geographic {
    [Serializable]
    public abstract class GeopoliticalLocation {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("alpha_2")]
        public string Alpha2 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subdivisions")]
        public List<Subdivision> Subdivisions { get; set; }

        [JsonProperty("type")]
        public abstract string Type { get; set; }

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
    }
}
