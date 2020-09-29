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

    [RoutePrefix("api/login")]
    public class LoginController : ApiController {
        public class LoginArgument {
            public string email;
            public string password;
        }

        [HttpPost, Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginArgument argument) {
            User user = await DAO.User.GetByEmail(argument.email);
            if (user == null || !user.VerifyPassword(argument.password)) {
                return Unauthorized();
            }
            user.NewSessionCookie();
            await DAO.User.Update(user);
            return Ok(user.SessionCookie);
        }
    }
}
