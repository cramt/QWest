using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;

namespace Model {
    [Serializable]
    public class Group : INotifyPropertyChanged {
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
        private DateTime _creationTime;
        [JsonProperty("creationTime")]
        public DateTime CreationTime {
            get => _creationTime;
            set {
                _creationTime = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string _description;
        [JsonProperty("description")]
        public string Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private ProgressMap _progressMap;
        [JsonProperty("progressMap")]
        public ProgressMap ProgressMap {
            get => _progressMap;
            set {
                _progressMap = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private List<User> _members;
        [JsonProperty("members")]
        public List<User> Members {
            get => _members;
            set {
                _members = value;
                OnPropertyChanged();
            }
        }
        public Group(string name, int creationTime, string description, ProgressMap progressMap, IEnumerable<User> members)
            : this(name, creationTime.ToUnsigned(), description, progressMap, members) {

        }
        public Group(string name, uint creationTime, string description, ProgressMap progressMap, IEnumerable<User> members)
            : this(name, creationTime.ToDate(), description, progressMap, members) {

        }
        public Group(string name, DateTime creationTime, string description, ProgressMap progressMap, IEnumerable<User> members)
            : this(name, creationTime, description, progressMap, members, null) {

        }

        public Group(string name, int creationTime, string description, ProgressMap progressMap, IEnumerable<User> members, int? id)
            : this(name, creationTime.ToUnsigned(), description, progressMap, members, id) {

        }
        public Group(string name, uint creationTime, string description, ProgressMap progressMap, IEnumerable<User> members, int? id)
            : this(name, creationTime.ToDate(), description, progressMap, members, id) {

        }
        public Group(string name, DateTime creationTime, string description, ProgressMap progressMap, IEnumerable<User> members, int? id) {
            Name = name;
            CreationTime = creationTime;
            Description = description;
            ProgressMap = progressMap;
            Members = members.MapValue(x => x.ToList());
            Id = id;
        }

        [JsonConstructor]
        public Group() { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
