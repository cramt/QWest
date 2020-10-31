using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utilities;

namespace Model {
    public class PostUpload {
        public string Contents { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public DateTime PostTime { get; set; }
        [JsonIgnore]
        public List<byte[]> Images { get; set; }
        public int? Location { get; set; }
        public PostUpload(string contents, User user, int postTime, List<byte[]> images)
            : this(contents, user, postTime.ToUnsigned(), images, null) {

        }
        public PostUpload(string contents, User user, uint postTime, List<byte[]> images)
            : this(contents, user, postTime, images, null) {
        }
        public PostUpload(string contents, User user, uint postTime, List<byte[]> images, int? location)
            : this(contents, user, postTime.ToDate(), images, location) {
        }
        public PostUpload(string contents, User user, DateTime postTime, List<byte[]> images)
            : this(contents, user, postTime, images, null) {

        }
        public PostUpload(string contents, User user, DateTime postTime, List<byte[]> images, int? location) {
            Contents = contents;
            User = user;
            PostTime = postTime;
            Images = images;
            Location = location;
        }
    }
}
