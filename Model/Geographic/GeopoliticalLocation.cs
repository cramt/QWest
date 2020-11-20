using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Model.Geographic {
    [Serializable]
    public abstract class GeopoliticalLocation : INotifyPropertyChanged {
        [JsonIgnore]
        private int? _id;
        [JsonProperty("id")]
        public int? Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        private string _alpha2;
        [JsonProperty("alpha_2")]
        public string Alpha2 {
            get => _alpha2;
            set {
                _alpha2 = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string _name;
        [JsonProperty("name")]
        public string Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private List<string> _names;
        [JsonProperty("names")]
        public List<string> Names {
            get => _names;
            set {
                _names = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private List<Subdivision> _subdivisions;
        [JsonProperty("subdivisions")]
        public List<Subdivision> Subdivisions {
            get => _subdivisions; 
            set {
                _subdivisions = value;
                OnPropertyChanged();
            }
        }

        public static List<Country> Parse(string json) {
            List<Country> countries = JsonConvert.DeserializeObject<List<Country>>(json);
            Action<GeopoliticalLocation, Subdivision> traverse = null;
            traverse = (parent, curr) => {
                curr.Parent = parent;
                curr.Subdivisions.ForEach(x => traverse(curr, x));
            };
            countries.ForEach(x => x.Subdivisions.ForEach(y => traverse(x, y)));
            return countries;
        }

        public static IEnumerable<GeopoliticalLocation> Traverse(IEnumerable<Country> countries) {
            return countries.Select(Traverse).SelectMany(x => x);
        }

        public static IEnumerable<GeopoliticalLocation> Traverse(GeopoliticalLocation location) {
            return location.Subdivisions.Select(Traverse).SelectMany(x => x).Concat(new GeopoliticalLocation[] { location });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
