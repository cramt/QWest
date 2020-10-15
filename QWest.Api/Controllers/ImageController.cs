using Model;
using QWest.DataAcess;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class ImageController : ApiController {
        [ResponseType(typeof(string))]
        public async Task<HttpResponseMessage> Get(int id) {
            byte[] image = await DAO.Image.Get(id);
            if(image == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(new MemoryStream(image));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            return result;
        }
    }
}
