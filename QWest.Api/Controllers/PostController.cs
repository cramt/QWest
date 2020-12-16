using Model;
using QWest.Api;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Windows.Forms.VisualStyles;
using static Utilities.Utilities;

namespace QWest.Apis {
    public class PostController : ApiController {

        private DAO.IPost _postRepo = null;
        public DAO.IPost PostRepo {
            get {
                if (_postRepo == null) {
                    _postRepo = DAO.Post;
                }
                return _postRepo;
            }
            set {
                _postRepo = value;
            }
        }

        private DAO.IGroup _groupRepo = null;
        public DAO.IGroup GroupRepo {
            get {
                if (_groupRepo == null) {
                    _groupRepo = DAO.Group;
                }
                return _groupRepo;
            }
            set {
                _groupRepo = value;
            }
        }

        public class UploadArgument {
            public string contents;
            public int? location;
            public List<string> images;
            public int? groupAuthor;

            public async Task<List<byte[]>> ParseImages() {
                if(images == null) {
                    return new List<byte[]>();
                }
                return (await Task.WhenAll(images.Select(x => Task.Factory.StartNew(() => {
                    if (x.Contains(",")) {
                        x = x.Split(',')[1];
                    }
                    Image image = Image.FromStream(new MemoryStream(Convert.FromBase64String(x)));
                    MemoryStream stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);
                    return stream.ToArray();
                })))).ToList();
            }
        }

        [HttpPost]
        [ResponseType(typeof(Post))]
        public async Task<HttpResponseMessage> Upload([FromBody] UploadArgument upload) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            List<byte[]> images = await upload.ParseImages();
            string contents = upload.contents.Trim();
            Post post;
            if (upload.groupAuthor == null) {
                post = await PostRepo.Add(contents, user, images, upload.location);
            }
            else {
                int groupAuthor = (int)upload.groupAuthor;
                if (await GroupRepo.IsMember(groupAuthor, user)) {
                    post = await PostRepo.AddGroupAuthor(contents, groupAuthor, images, upload.location);
                }
                else {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, post);
        }
        [ResponseType(typeof(List<Post>))]
        public async Task<HttpResponseMessage> GetUsersPosts(int? id) {
            if (id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                id = user.Id;
            }
            return Request.CreateResponse(HttpStatusCode.OK, await PostRepo.GetByUserId(id ?? 0));
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Update([FromBody] Post post) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            if (!(await PostRepo.IsAuthor(user, post))) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await PostRepo.Update(post);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Update([FromBody] UploadArgument upload)
        {
            User user = Request.GetOwinContext().Get<User>("user");
            Group group = Request.GetOwinContext().Get<Group>("group");
            if (user == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            string contents = upload.contents.Trim();
            //List<byte[]> images = await upload.ParseImages();
            uint upostTime = DateTime.Now.ToUint();
            int postTime = upostTime.ToSigned();
            Post post;
            if (upload.groupAuthor == null)
            {
                post = new Post(contents, user, null, postTime, null, upload.location);
            }
            else
            {
                int groupAuthor = (int)upload.groupAuthor;
                if (await GroupRepo.IsMember(groupAuthor, user))
                {
                    post = new Post(contents, null, group, postTime, null, upload.location);
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            if (!(await PostRepo.IsAuthor(user, post)))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await PostRepo.Update(post);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        [HttpGet]
        [ResponseType(typeof(List<Post>))]
        public async Task<HttpResponseMessage> GetFeed(int? id = null, int amount = 20, int offset = 0) {
            int finalId;
            if (id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)id;
            }
            return Request.CreateResponse(HttpStatusCode.OK, await PostRepo.GetFeedByUserId(finalId, amount, offset));
        }

        [HttpGet]
        [ResponseType(typeof(List<Post>))]
        public async Task<HttpResponseMessage> GetGroupPosts(int id, int amount = 20, int offset = 0)
        {
            if ((await GroupRepo.Get(id)) == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, await PostRepo.GetGroupFeedById(id, amount, offset));
        }

        [HttpGet]
        [ResponseType(typeof(Post))]
        public async Task<HttpResponseMessage> Get(int id) {
            Post post = await PostRepo.Get(id);
            if(post == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, post);
        }
    }
}
