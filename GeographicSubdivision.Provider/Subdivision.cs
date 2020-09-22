using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeographicSubdivision.Provider {
    [Serializable]
    public class Subdivision : ISubdividable {
        [JsonProperty("code")]
        private readonly string _code;

        [JsonProperty("name")]
        private readonly string _name;

        [JsonProperty("type")]
        private readonly string _type;

        [JsonProperty("subdivision")]
        private readonly List<Subdivision> _subdivision;

        [JsonProperty("names")]
        private readonly List<string> _names;

        internal Subdivision _parent;

        internal Country _parentCountry;

        public string Code { get { return _code; } }

        public string Name { get { return _name; } }

        public string Type { get { return _type; } }

        public List<Subdivision> Subdivisions { get { return _subdivision; } }

        public Subdivision Parent { get { return _parent; } }

        public Country ParentCountry { get { return _parentCountry; } }

        public List<string> Names { get { return _names; } }

        public string GetFullId() {
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
