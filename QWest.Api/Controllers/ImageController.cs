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
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class ImageController : ApiController {

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

        [ResponseType(typeof(string))]
        public async Task<HttpResponseMessage> Get(int? id) {
            Stream stream = null;
            if (id == null) {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QWest.Api.res.silhouette-profile-picture.jpg");
            }
            else {
                byte[] image = await ImageRepo.Get((int)id);
                if (image == null) {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                stream = new MemoryStream(image);
            }
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            if (!Utilities.Utilities.DebugMode) {
                result.Headers.CacheControl = new CacheControlHeaderValue {
                    MaxAge = new TimeSpan(8765, 0, 0)
                };
            }
            return result;
        }
    }
}
