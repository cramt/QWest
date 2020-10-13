using Model;
using QWest.DataAcess;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class UserController : ApiController {
        [ResponseType(typeof(User))]
        public async Task<HttpResponseMessage> Id(int id) {
            User user = await DAO.User.Get(id);
            user.SessionCookie = null;
            if(user == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, user);
            }
        }
    }
}
