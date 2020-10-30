using Newtonsoft.Json;
using System;

namespace Model.Geographic {
    [Serializable]
    public class Country : GeopoliticalLocation {

        [JsonProperty("alpha_3")]
        public string Alpha3 { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("sub_region")]
        public string SubRegion { get; set; }

        [JsonProperty("intermediate_region")]
        public string IntermediateRegion { get; set; }

        [JsonProperty("region_code")]
        public int? RegionCode { get; set; }

        [JsonProperty("sub_region_code")]
        public int? SubRegionCode { get; set; }

        [JsonProperty("intermediate_region_code")]
        public int? IntermediateRegionCode { get; set; }
    }
}
