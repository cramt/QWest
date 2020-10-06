using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeographicSubdivision.Provider {
    [Serializable]
    public class Country : AbstractLocation {

        [JsonProperty("alpha_2")]
        private readonly string _alpha2;

        [JsonProperty("alpha_3")]
        private readonly string _alpha3;

        [JsonProperty("common_name")]
        private readonly string _commonName;

        [JsonProperty("numeric")]
        private readonly string _numeric;

        [JsonProperty("official_name")]
        private readonly string _officialName;

        internal async Task SetBackwardsReference() {
            await Task.WhenAll(Subdivisions.Select(subdivision => SetBackwardsReference(null, subdivision)).ToArray());
        }

        private async Task SetBackwardsReference(Subdivision parent, Subdivision subdivision) {
            subdivision._parent = parent;
            subdivision._parentCountry = this;
            await Task.WhenAll(subdivision.Subdivisions.Select(subsubdivision => SetBackwardsReference(subdivision, subsubdivision)).ToArray());
        }

        [JsonIgnore]
        public string CountryCode { get { return _alpha2; } }

        [JsonIgnore]
        public string Alpha2 { get { return _alpha2; } }

        [JsonIgnore]
        public string Alpha3 { get { return _alpha3; } }

        [JsonIgnore]
        public string CommonName {
            get {
                if (_commonName == null) {
                    if (Name == null) {
                        return _officialName;
                    }
                    return Name;
                }
                return _commonName;
            }
        }
        [JsonIgnore]
        public string Numeric { get { return _numeric; } }

        [JsonIgnore]
        public string OfficialName {
            get {
                if (_officialName == null) {
                    if (Name == null) {
                        return _commonName;
                    }
                    return Name;
                }
                return _officialName;
            }
        }
        public override string Type {
            get {
                return "Country";
            }
        }
        public override string GetFullId() {
            return Alpha2;
        }
    }
}
