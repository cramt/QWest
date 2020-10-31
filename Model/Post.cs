﻿using Model.Geographic;
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
        public GeopoliticalLocation Location { get; set; }
        public Post(string contents, User user, int postTime, List<int> images, int? id)
            : this(contents, user, postTime.ToUnsigned(), images, null, id) {

        }
        public Post(string contents, User user, uint postTime, List<int> images)
            : this(contents, user, postTime, images, null) {
        }
        public Post(string contents, User user, int postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, user, postTime.ToUnsigned(), images, locationId, null) {
        }
        public Post(string contents, User user, uint postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, user, postTime, images, locationId, null) {
        }
        public Post(string contents, User user, int postTime, List<int> images, GeopoliticalLocation locationId, int? id)
            : this(contents, user, postTime.ToUnsigned(), images, locationId, id) {
        }
        public Post(string contents, User user, uint postTime, List<int> images, GeopoliticalLocation locationId, int? id)
            : this(contents, user, postTime.ToDate(), images, locationId, id) {
        }
        public Post(string contents, User user, DateTime postTime, List<int> images)
            : this(contents, user, postTime, images, null) {

        }

        public Post(string contents, User user, DateTime postTime, List<int> images, GeopoliticalLocation locationId)
            : this(contents, user, postTime, images, locationId, null) {

        }
        public Post(string contents, User user, DateTime postTime, List<int> images, GeopoliticalLocation locationId, int? id) {
            Contents = contents;
            User = user;
            PostTime = postTime;
            Images = images;
            Id = id;
            Location = locationId;
        }
    }
}
