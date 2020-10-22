using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Geographic {
    [Serializable]
    public class Subdivision : GeopoliticalLocation {
        [JsonProperty("type")]
        public override string Type { get; set; }
    }
}
