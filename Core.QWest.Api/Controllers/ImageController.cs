using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Model;
using QWest.DataAccess;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase {

        private DAO.IImage _imageRepo = null;
        public DAO.IImage ImageRepo {
            get {
                if (_imageRepo == null) {
                    _imageRepo = DAO.Image;
                }
                return _imageRepo;
            }
            set {
                _imageRepo = value;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int? id) {
            byte[] data;
            if (id == null) {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Core.QWest.Api.res.silhouette-profile-picture.jpg");
                using (var memoryStream = new MemoryStream()) {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }
            else {
                byte[] image = await ImageRepo.Get((int)id);
                if (image == null) {
                    return NotFound();
                }
                data = image;
            }
            FileContentResult result = new FileContentResult(data, "image/jpeg");
            
            if (!Utilities.Utilities.DebugMode) {
                Response.Headers.Add("Cache-Control", new StringValues("max-age=31554000"));
            }
            return result;
        }
    }
}
