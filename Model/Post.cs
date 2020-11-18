using Model.Geographic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utilities;

namespace Model {
    public class Post {
        public int? Id { get; set; }
        public string Contents { get; set; }
        public User UserAuthor { get; set; }
        public Group GroupAuthor { get; set; }
        public DateTime PostTime { get; set; }
        public List<int> Images { get; set; }
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
    }
}
