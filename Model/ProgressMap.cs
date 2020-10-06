using GeographicSubdivision.Provider;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Model {
    public class ProgressMap {
        public int? Id { get; set; }
        public List<string> Locations { get; set; }
        [JsonIgnore]
        public IEnumerable<IReadOnlyCollection<AbstractLocation>> LocationsTransformed {
            get => Locations.Select(x => GeographyProvider.Instance.Alpha2Map[x]);
        }
        public ProgressMap() : this(new List<string>()) {

        }
        public ProgressMap(List<string> locations) : this(locations, null) {

        }
        public ProgressMap(List<string> locations, int? id) {
            Locations = locations;
            Id = id;
        }
    }
}
