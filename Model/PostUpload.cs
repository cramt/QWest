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
        public PostUpload(string contents, User user, int PostUploadTime, List<byte[]> images, int? id)
            : this(contents, user, unchecked((uint)PostUploadTime), images, null, id) {

        }
        public PostUpload(string contents, User user, uint PostUploadTime, List<byte[]> images)
            : this(contents, user, PostUploadTime, images, null) {
        }
        public PostUpload(string contents, User user, uint PostUploadTime, List<byte[]> images, string locationId)
            : this(contents, user, PostUploadTime, images, locationId, null) {
        }
        public PostUpload(string contents, User user, uint PostUploadTime, List<byte[]> images, string locationId, int? id)
            : this(contents, user, Config.Config.Instance.StartDate.AddSeconds(PostUploadTime), images, locationId, id) {
        }
        public PostUpload(string contents, User user, DateTime PostUploadTime, List<byte[]> images)
            : this(contents, user, PostUploadTime, images, null) {

        }

        public PostUpload(string contents, User user, DateTime PostUploadTime, List<byte[]> images, string locationId)
            : this(contents, user, PostUploadTime, images, locationId, null) {

        }
        public PostUpload(string contents, User user, DateTime PostUploadTime, List<byte[]> images, string locationId, int? id) {
            Contents = contents;
            User = user;
            PostUploadTime = PostUploadTime;
            Images = images;
            Id = id;
            LocationId = locationId;
        }
    }
}
