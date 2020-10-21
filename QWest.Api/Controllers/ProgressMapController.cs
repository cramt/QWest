using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Api.Controllers {
    public class ProgressMapController : ApiController {
        [HttpGet]
        [ResponseType(typeof(ProgressMap))]
        public Task<HttpResponseMessage> UserId() {
            return UserId(null);
        }
        [HttpGet]
        [ResponseType(typeof(ProgressMap))]
        public async Task<HttpResponseMessage> UserId(int? id) {
            if (id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                return Request.CreateResponse(HttpStatusCode.OK, await DAO.ProgressMap.Get(user));
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, await DAO.ProgressMap.GetByUserId((int)id));
            }
        }
        public class UpdateArgument {
            public int id;
            public List<string> additions;
            public List<string> subtractions;
        }
        [HttpPost]
        [ResponseType(typeof(bool))]
        public async Task<HttpResponseMessage> Change([FromBody] UpdateArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            ProgressMap map = await DAO.ProgressMap.Get(user);
            if(argument.id != map.Id) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await DAO.ProgressMap.Update(argument.id, argument.additions, argument.subtractions);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}
