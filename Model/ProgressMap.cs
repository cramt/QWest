using GeographicSubdivision.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Model {
    public class ProgressMap {
        public int? Id { get; set; }
        public List<string> Locations { get; set; }
        public IEnumerable<IReadOnlyCollection<ISubdividable>> LocationsTransformed {
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
