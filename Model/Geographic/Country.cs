using Newtonsoft.Json;
using System;

namespace Model.Geographic {
    [Serializable]
    public class Country : GeopoliticalLocation {
        public override string Type {
            get {
                return "Country";
            }
            set {

            }
        }

        [JsonProperty("alpha_3")]
        public string Alpha3 { get; set; }

        [JsonProperty("official_name")]
        public string OfficialName { get; set; }

        [JsonProperty("common_name")]
        public string CommonName { get; set; }

        [JsonProperty("numeric")]
        public int Numeric { get; set; }
    }
}
