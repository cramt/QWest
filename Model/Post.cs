using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utilities;

namespace Model {
    public class Post {
        public int? Id { get; set; }
        public string Contents { get; set; }
        public User User { get; set; }
        public DateTime PostTime { get; set; }
        public List<int> Images { get; set; }
        public string LocationId { get; set; }
        public Post(string contents, User user, int postTime, List<int> images)
            : this(contents, user, postTime, images, null) {

        }
        public Post(string contents, User user, int postTime, List<int> images, int? id)
            : this(contents, user, unchecked((uint)postTime), images, null, id) {

        }
        public Post(string contents, User user, uint postTime, List<int> images)
            : this(contents, user, postTime, images, null) {
        }
        public Post(string contents, User user, uint postTime, List<int> images, string locationId)
            : this(contents, user, postTime, images, locationId, null) {
        }
        public Post(string contents, User user, uint postTime, List<int> images, string locationId, int? id)
            : this(contents, user, Config.Config.Instance.StartDate.AddSeconds(postTime), images, locationId, id) {
        }
        public Post(string contents, User user, DateTime postTime, List<int> images)
            : this(contents, user, postTime, images, null) {

        }

        public Post(string contents, User user, DateTime postTime, List<int> images, string locationId)
            : this(contents, user, postTime, images, locationId, null) {

        }
        public Post(string contents, User user, DateTime postTime, List<int> images, string locationId, int? id) {
            Contents = contents;
            User = user;
            PostTime = postTime;
            Images = images;
            Id = id;
            LocationId = locationId;
        }
    }
}
