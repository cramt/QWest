using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model {
    public class ProgressMap {
        public int? Id { get; set; }

        public Collection<int> Locations { get; }

        [JsonIgnore]
        public List<int> Additions { get; } = new List<int>();

        [JsonIgnore]
        public List<int> Subtractions { get; } = new List<int>();

        public ProgressMap() : this(new List<int>()) {

        }
        public ProgressMap(List<int> locations) : this(locations, null) {

        }
        public ProgressMap(List<int> locations, int? id) : this(new ObservableCollection<int>(locations), id) {

        }

        public ProgressMap(ObservableCollection<int> locations, int? id) {
            locations.CollectionChanged += (o, e) => {
                foreach(int item in e.NewItems) {
                    if (Subtractions.Contains(item)) {
                        Subtractions.Remove(item);
                    }
                    else {
                        Additions.Add(item);
                    }
                }
                foreach (int item in e.OldItems) {
                    if (Additions.Contains(item)) {
                        Additions.Remove(item);
                    }
                    else {
                        Subtractions.Add(item);
                    }
                }
            };
            Locations = locations;
            Id = id;
        }
    }
}
