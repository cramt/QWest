using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GeographicSubdivision.Provider {
    [Serializable]
    public class Subdivision : AbstractLocation {
        [JsonProperty("code")]
        private readonly string _code;

        [JsonProperty("type")]
        private readonly string _type;

        [JsonIgnore]
        internal Subdivision _parent;

        [JsonIgnore]
        internal Country _parentCountry;

        [JsonIgnore]
        public string Code { get { return _code; } }

        public override string Type { get { return _type; } }

        [JsonIgnore]
        public Subdivision Parent { get { return _parent; } }

        [JsonIgnore]
        public Country ParentCountry { get { return _parentCountry; } }


        public override string GetFullId() {
            string parentId;
            if (_parent == null) {
                parentId = _parentCountry.GetFullId();
            }
            else {
                parentId = _parent.GetFullId();
            }
            return Code + "-" + parentId;
        }
    }
}
