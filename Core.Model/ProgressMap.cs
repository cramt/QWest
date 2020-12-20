using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace Model {
    public class ProgressMap {
        public int? Id { get; set; }

        [JsonIgnore]
        private ObservedList<int> _locations;
        public ICollection<int> Locations { get { return _locations; } }

        [JsonIgnore]
        public List<int> Additions { get { return _locations.Additions; } }

        [JsonIgnore]
        public List<int> Subtractions { get { return _locations.Subtractions; } }

        public ProgressMap() : this(new List<int>()) {

        }
        public ProgressMap(List<int> locations) : this(locations, null) {

        }
        public ProgressMap(List<int> locations, int? id) : this(new ObservedList<int>(locations), id) {

        }

        public ProgressMap(ObservedList<int> locations, int? id) {
            _locations = locations;
            Id = id;
        }
    }
}
