using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Model {
    public class Group {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Description { get; set; }
        public ProgressMap ProgressMap { get; set; }
        public List<User> Members { get; set; }
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
            Members = members.ToList();
            Id = id;
        }

        [JsonConstructor]
        public Group() { }
    }
}
