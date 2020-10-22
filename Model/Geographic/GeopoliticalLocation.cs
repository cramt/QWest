using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Geographic {
    [Serializable]
    public abstract class GeopoliticalLocation {
        [JsonProperty("alpha_2")]
        public string Alpha2 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subdivisions")]
        public List<Subdivision> Subdivisions { get; set; }

        [JsonProperty("type")]
        public abstract string Type { get; set; }
    }
}
