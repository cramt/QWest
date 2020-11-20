using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Model.Geographic {
    [Serializable]
    public class Subdivision : GeopoliticalLocation, INotifyPropertyChanged {
        [JsonIgnore]
        public int _superId;
        [JsonIgnore]
        public int SuperId {
            get => _superId;
            set {
                _superId = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private GeopoliticalLocation _parent;
        [JsonIgnore]
        public GeopoliticalLocation Parent {
            get => _parent;
            set {
                _parent = value;
                OnPropertyChanged();
            }
        }

    }
}
