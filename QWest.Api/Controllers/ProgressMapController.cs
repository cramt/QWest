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

namespace QWest.Api.Controllers {
    public class ProgressMapController : ApiController {
        [HttpGet]
        public async Task<ProgressMap> UserId(int id) {
            return await DAO.ProgressMap.GetByUserId(id);
        }
        [HttpGet]
        public async Task<ProgressMap> Mine() {
            User user = Request.GetOwinContext().Get<User>("user");
            if(user == null) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            return await DAO.ProgressMap.Get(user);
        }
    }
}
