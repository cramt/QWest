﻿using Model;
using QWest.DataAcess;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace QWest.Apis {

    [RoutePrefix("login")]
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
        [HttpGet, Route("get_me")]
        public IHttpActionResult GetMe() {
            User user = Request.GetOwinContext().Get<User>("user");
            if(user == null) {
                return NotFound();
            }
            else {
                return Ok(user);
            }
        }
    }
}