﻿using Model;
using QWest.Api;
using QWest.DataAcess;
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
using static Utilities.Utilities;

namespace QWest.Apis {
    public class PostController : ApiController {
        [HttpPost]
        [ResponseType(typeof(Post))]
        public async Task<HttpResponseMessage> Upload([FromBody] PostUpload upload) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            upload.User = user;
            if (Request.Content.IsMimeMultipartContent()) {
                upload.Images = await Utils.GetImages(Request);
            }
            else {
                upload.Images = new List<byte[]>();
            }
            Post post = await DAO.Post.Add(upload);
            return Request.CreateResponse(HttpStatusCode.OK, post);
        }
        [ResponseType(typeof(List<Post>))]
        public async Task<HttpResponseMessage> GetUsersPosts(int? id) {
            if(id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                id = user.Id;
            }
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Post.GetByUserId(id ?? 0));
        }
    }
}