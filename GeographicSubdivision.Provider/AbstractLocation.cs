using Newtonsoft.Json;
using System.Collections.Generic;

namespace GeographicSubdivision.Provider {
    public abstract class AbstractLocation {
        [JsonProperty("name")]
        private readonly string _name;

        [JsonProperty("names")]
        private readonly List<string> _names;

        [JsonProperty("subdivision")]
        private readonly List<Subdivision> _subdivision;

        [JsonIgnore]
        public string Name {
            get {
                return _name;
            }
        }

        [JsonIgnore]
        public List<string> Names {
            get {
                if (_names == null) {
                    return new List<string> { _name };
                }
                else {
                    return _names;
                }
            }
        }

        [JsonIgnore]
        public List<Subdivision> Subdivisions {
            get {
                return _subdivision;
            }
        }

        [JsonIgnore]
        public abstract string Code { get; }

        [JsonIgnore]
        public abstract string Type { get; }
        public abstract string GetFullId();
    }
}
