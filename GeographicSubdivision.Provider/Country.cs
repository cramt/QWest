using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GeographicSubdivision.Provider {
    [Serializable]
    public class Country : ISubdividable {

        [JsonProperty("alpha_2")]
        private readonly string _alpha2;

        [JsonProperty("alpha_3")]
        private readonly string _alpha3;

        [JsonProperty("common_name")]
        private readonly string _commonName;

        [JsonProperty("name")]
        private readonly string _name;

        [JsonProperty("numeric")]
        private readonly string _numeric;

        [JsonProperty("official_name")]
        private readonly string _officialName;

        [JsonProperty("subdivision")]
        private readonly List<Subdivision> _subdivision;

        internal async Task SetBackwardsReference() {
            await Task.WhenAll(Subdivisions.Select(subdivision => SetBackwardsReference(null, subdivision)).ToArray());
        }

        private async Task SetBackwardsReference(Subdivision parent, Subdivision subdivision) {
            subdivision._parent = parent;
            subdivision._parentCountry = this;
            await Task.WhenAll(subdivision.Subdivisions.Select(subsubdivision => SetBackwardsReference(subdivision, subsubdivision)).ToArray());
        }
        public string CountryCode { get { return _alpha2; } }
        public string Alpha2 { get { return _alpha2; } }
        public string Alpha3 { get { return _alpha3; } }
        public string CommonName {
            get {
                if (_commonName == null) {
                    if (_name == null) {
                        return _officialName;
                    }
                    return _name;
                }
                return _commonName;
            }
        }
        public string Name { get { return _name; } }
        public string Numeric { get { return _numeric; } }
        public string OfficialName {
            get {
                if (_officialName == null) {
                    if (_name == null) {
                        return _commonName;
                    }
                    return _name;
                }
                return _officialName;
            }
        }
        public List<Subdivision> Subdivisions { get { return _subdivision; } }

        public List<string> Names {
            get {
                return new string[] { _name, _officialName, _commonName }.ToList().FindAll(x => x != null).ToList();
            }
        }

        public string Type {
            get {
                return "Country";
            }
        }

        public string GetFullId() {
            return Alpha2;
        }
    }
}
