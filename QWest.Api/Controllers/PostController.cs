using Microsoft.AspNetCore.Mvc;
using Model;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase {

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
                if (images == null) {
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

        [HttpPost("upload")]
        public async Task<ActionResult<Post>> Upload([FromBody] UploadArgument upload) {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
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
                    return Unauthorized();
                }
            }
            return Ok(post);
        }
        [HttpGet("user/{id}")]
        public async Task<ActionResult<List<Post>>> GetUsersPosts(int? id) {
            if (id == null) {
                User user = Request.GetUser();
                if (user == null) {
                    return Unauthorized();
                }
                id = user.Id;
            }
            return Ok(await PostRepo.GetByUserId(id ?? 0));
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update([FromBody] Post post) {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            if (!(await PostRepo.IsAuthor(user, post))) {
                return Unauthorized();
            }
            await PostRepo.Update(post);
            return Ok();
        }

        [HttpGet("feed/{id}/{amount}/{offset}")]
        public async Task<ActionResult<List<Post>>> GetFeed(int? id = null, int amount = 20, int offset = 0) {
            int finalId;
            if (id == null) {
                User user = Request.GetUser();
                if (user == null) {
                    return Unauthorized();
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)id;
            }
            return Ok(await PostRepo.GetFeedByUserId(finalId, amount, offset));
        }

        [HttpGet("group/{id}/{amount}/{offset}")]
        public async Task<ActionResult<List<Post>>> GetGroupPosts(int id, int amount = 20, int offset = 0) {
            if ((await GroupRepo.Get(id)) == null) {
                return NotFound();
            }
            return Ok(await PostRepo.GetGroupFeedById(id, amount, offset));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> Get(int id) {
            Post post = await PostRepo.Get(id);
            if (post == null) {
                return NotFound();
            }
            return Ok(post);
        }
    }
}
