using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utilities;

namespace Model {
    public class PostUpload {
        public int? Id { get; set; }
        public string Contents { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public DateTime PostTime { get; set; }
        [JsonIgnore]
        public List<byte[]> Images { get; set; }
        public string LocationId { get; set; }
        public PostUpload(string contents, User user, int PostUploadTime, List<byte[]> images)
            : this(contents, user, PostUploadTime, images, null) {

        }
        public PostUpload(string contents, User user, int postTime, List<byte[]> images, int? id)
            : this(contents, user, unchecked((uint)postTime), images, null, id) {

        }
        public PostUpload(string contents, User user, uint postTime, List<byte[]> images)
            : this(contents, user, postTime, images, null) {
        }
        public PostUpload(string contents, User user, uint postTime, List<byte[]> images, string locationId)
            : this(contents, user, postTime, images, locationId, null) {
        }
        public PostUpload(string contents, User user, uint postTime, List<byte[]> images, string locationId, int? id)
            : this(contents, user, Config.Config.Instance.StartDate.AddSeconds(postTime), images, locationId, id) {
        }
        public PostUpload(string contents, User user, DateTime postTime, List<byte[]> images)
            : this(contents, user, postTime, images, null) {

        }

        public PostUpload(string contents, User user, DateTime postTime, List<byte[]> images, string locationId)
            : this(contents, user, postTime, images, locationId, null) {

        }
        public PostUpload(string contents, User user, DateTime postTime, List<byte[]> images, string locationId, int? id) {
            Contents = contents;
            User = user;
            PostTime = postTime;
            Images = images;
            Id = id;
            LocationId = locationId;
        }
    }
}
