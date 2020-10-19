using Model;
using QWest.DataAcess;
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
        [ResponseType(typeof(string))]
        public async Task<HttpResponseMessage> Get(int? id) {
            Stream stream = null;
            if(id == null || id == 0) {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().Find(x => x.EndsWith("silhouette-profile-picture.jpg")));
            }
            else {
                byte[] image = await DAO.Image.Get((int)id);
                if (image == null) {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                stream = new MemoryStream(image);
            }
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            result.Content.Headers.Add("Cache-Control", "max-age=31536000"); //one year
            return result;
        }
    }
}
