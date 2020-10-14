using Model;
using QWest.DataAcess;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class LoginController : ApiController {
        public class LoginArgument {
            public string email;
            public string password;
        }

        [HttpPost]
        [ResponseType(typeof(string))]
        public async Task<HttpResponseMessage> Login([FromBody] LoginArgument argument) {
            User user = await DAO.User.GetByEmail(argument.email);
            if (user == null || !user.VerifyPassword(argument.password)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await DAO.User.SetNewSessionCookie(user);
            return Request.CreateResponse(HttpStatusCode.OK, user.SessionCookie);
        }
        [ResponseType(typeof(User))]
        public HttpResponseMessage GetMe() {
            User user = Request.GetOwinContext().Get<User>("user");
            if(user == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, user);
            }
        }
    }
}
