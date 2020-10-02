using Model;
using QWest.DataAcess;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace QWest.Apis {
    public class LoginController : ApiController {
        public class LoginArgument {
            public string email;
            public string password;
        }

        [HttpPost]
        public async Task<string> Login([FromBody] LoginArgument argument) {
            User user = await DAO.User.GetByEmail(argument.email);
            if (user == null || !user.VerifyPassword(argument.password)) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            user.NewSessionCookie();
            await DAO.User.Update(user);
            return user.SessionCookie;
        }
        public User GetMe() {
            User user = Request.GetOwinContext().Get<User>("user");
            if(user == null) {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else {
                return user;
            }
        }
    }
}
