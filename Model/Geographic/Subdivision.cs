using Newtonsoft.Json;
using System;

namespace Model.Geographic {
    [Serializable]
    public class Subdivision : GeopoliticalLocation {
        [JsonIgnore]
        public int SuperId { get; set; }

        [JsonIgnore]
        public GeopoliticalLocation Parent { get; set; }

    }
}
