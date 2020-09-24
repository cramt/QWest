using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;

namespace QWest.Apis {

    [RoutePrefix("api/sign_up")]
    public class SignUpController : ApiController {
        public class RegisterArgument {
            public string email;
            public string password;
            public string username;

            public User ToUser() {
                return new User(username, password, email).NewSessionCookie();
            }
        }

        [HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register([FromBody] RegisterArgument argument) {
            User user = argument.ToUser();
            await DAO.User.Add(user);
            return Ok(user);
        }
    }
}
