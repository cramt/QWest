﻿using GeographicSubdivision.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Model {
    [Serializable]
    public class ProgressMap {
        public int? Id { get; set; }

        public Collection<string> Locations { get; }

        [JsonIgnore]
        public IEnumerable<IReadOnlyCollection<AbstractLocation>> LocationsTransformed {
            get => Locations.Select(x => GeographyProvider.Instance.Alpha2Map[x]);
        }

        [JsonIgnore]
        public List<string> Additions { get; } = new List<string>();

        [JsonIgnore]
        public List<string> Subtractions { get; } = new List<string>();

        public ProgressMap() : this(new List<string>()) {

        }
        public ProgressMap(List<string> locations) : this(locations, null) {

        }
        public ProgressMap(List<string> locations, int? id) : this(new ObservableCollection<string>(locations), id) {

        }

        public ProgressMap(ObservableCollection<string> locations, int? id) {
            locations.CollectionChanged += (o, e) => {
                foreach(string item in e.NewItems) {
                    if (Subtractions.Contains(item)) {
                        Subtractions.Remove(item);
                    }
                    else {
                        Additions.Add(item);
                    }
                }
                foreach (string item in e.OldItems) {
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
