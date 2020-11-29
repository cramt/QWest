using Model;
using QWest.Api;
using QWest.DataAcess;
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

        public class UploadArgument {
            public string contents;
            public int? location;
            public List<string> images;

            public async Task<List<byte[]>> ParseImages() {
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
            Post post = await PostRepo.Add(upload.contents.Trim(), user, images, upload.location);
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
            if(user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            if (!(await PostRepo.IsAuthor(user, post))) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await PostRepo.Update(post);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [ResponseType(typeof(List<Post>))]
        public async Task<HttpResponseMessage> GetFeed(int? id, int amount, int offset) {
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
    }
}
