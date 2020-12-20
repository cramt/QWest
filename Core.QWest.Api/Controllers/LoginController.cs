using Microsoft.AspNetCore.Mvc;
using Model;
using QWest.Api;
using QWest.DataAccess;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase {

        private DAO.IUser _userRepo = null;
        public DAO.IUser UserRepo {
            get {
                if (_userRepo == null) {
                    _userRepo = DAO.User;
                }
                return _userRepo;
            }
            set {
                _userRepo = value;
            }
        }

        public class LoginArgument {
            public string email;
            public string password;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginArgument argument) {
            User user = await UserRepo.GetByEmail(argument.email);
            if (user == null || !user.VerifyPassword(argument.password)) {
                return Unauthorized();
            }
            await UserRepo.SetNewSessionCookie(user);
            return Ok(user.SessionCookie);
        }
        [HttpGet("me")]
        public ActionResult<User> GetMe() {
            User user = Request.GetUser();
            if(user == null) {
                return NotFound();
            }
            else {
                return Ok(user);
            }
        }
    }
}
