using Model.Geographic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace Model {
    [Serializable]
    public class Post {
        [JsonProperty("id")]
        public int? Id { get; set; }
        [JsonProperty("contents")]
        public string Contents { get; set; }
        [JsonProperty("userAuthor")]
        public User UserAuthor { get; set; }
        [JsonProperty("groupAuthor")]
        public Group GroupAuthor { get; set; }
        [JsonProperty("postTime")]
        public DateTime PostTime { get; set; }
        [JsonProperty("images")]
        public List<int> Images { get; set; }
        [JsonProperty("location")]
        public GeopoliticalLocation Location { get; set; }
        public Post(string contents, User userAuthor, Group groupAuthor, int postTime, List<int> images, int? id)
            : this(contents, userAuthor, groupAuthor, postTime.ToUnsigned(), images, null, id) {

        }
        public Post(string contents, User userAuthor, Group groupAuthor, uint postTime, List<int> images)
            : this(contents, userAuthor, groupAuthor, postTime, images, null) {
        }
        public Post(string contents, User userAuthor, Group groupAuthor, int postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, userAuthor, groupAuthor, postTime.ToUnsigned(), images, locationId, null) {
        }
        public Post(string contents, User userAuthor, Group groupAuthor, uint postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, userAuthor, groupAuthor, postTime, images, locationId, null) {
        }
        public Post(string contents, User userAuthor, Group groupAuthor, int postTime, List<int> images, GeopoliticalLocation locationId, int? id)
            : this(contents, userAuthor, groupAuthor, postTime.ToUnsigned(), images, locationId, id) {
        }
        public Post(string contents, User userAuthor, Group groupAuthor, uint postTime, List<int> images, GeopoliticalLocation locationId, int? id)
            : this(contents, userAuthor, groupAuthor, postTime.ToDate(), images, locationId, id) {
        }
        public Post(string contents, User userAuthor, Group groupAuthor, DateTime postTime, List<int> images)
            : this(contents, userAuthor, groupAuthor, postTime, images, null) {

        }

        public Post(string contents, User userAuthor, Group groupAuthor, DateTime postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, userAuthor, groupAuthor, postTime, images, locationId, null) {

        }
        public Post(string contents, User userAuthor, Group groupAuthor, DateTime postTime, List<int> images, GeopoliticalLocation locationId, int? id) {
            Contents = contents;
            UserAuthor = userAuthor;
            GroupAuthor = groupAuthor;
            PostTime = postTime;
            Images = images;
            Id = id;
            Location = locationId;
        }
        [JsonConstructor]
        public Post() {

        }
    }
}
