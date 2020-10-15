using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QWest.Api {
    static class Utils {
        public static async Task<List<byte[]>> GetImages(HttpRequestMessage request) {
            var provider = new MultipartMemoryStreamProvider();
            await request.Content.ReadAsMultipartAsync(provider);
            return (await Task.WhenAll(provider.Contents.Select(async file => {
                //TODO: use http 415
                //TODO: do something with this file.Headers.ContentType.MediaType
                Image image = Image.FromStream(await file.ReadAsStreamAsync());
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }))).ToList();
        }
    }
}
