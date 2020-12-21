using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;
using QWest.Api;
using QWest.DataAccess;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Utilities.Utilities;

namespace QWest.Apis {
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase {

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

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id) {
            User user = await UserRepo.Get(id);
            if (user == null) {
                return NotFound();
            }
            else {
                return Ok(user);
            }
        }

        [Serializable]
        public class NewUser {
            [JsonProperty("username")]
            public string Username;
            [JsonProperty("description")]
            public string Description;

            public User UpdateUser(User user) {
                if (Username != null)
                    user.Username = Username;
                if (Description != null)
                    user.Description = Description;
                return user;
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update([FromBody] NewUser newUser) {
            if (newUser == null) {
                return BadRequest();
            }
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            await UserRepo.Update(newUser.UpdateUser(user));
            return Ok();
        }

        [HttpPost("passwordreset")]
        public async Task<ActionResult<bool>> RequestPasswordReset() {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            string token = await DAO.PasswordResetToken.NewToken(user);
            Request.GetDisplayUrl();
            //TODO: fix later
            /*
            string url = "http://" + Request.RequestUri.Host + ":" + Config.Config.Instance.ServePort + "/password_reset.html?token=" + WebUtility.UrlEncode(token);
            await SendEmail(new EmailArgument(user.Email, "Password Reset", "You have requested a password reset, please go to this link " + url + " to finalize and save your new password"));
            */
            return Ok(true);
        }

        public class ConfirmPasswordResetArgument {
            public string token;
            public string password;
        }

        [HttpPost("confirm/passwordreset")]
        public async Task<ActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetArgument argument) {
            User user = await DAO.PasswordResetToken.GetUser(argument.token);
            if (user == null) {
                return Unauthorized();
            }
            user.NewPassword(argument.password);
            await Task.WhenAll(new Task[] { UserRepo.Update(user), DAO.PasswordResetToken.DeleteToken(argument.token) });
            return Ok();
        }

        [HttpGet("passwordreset/{token}")]
        public async Task<ActionResult<User>> GetByPasswordResetToken(string token) {
            if (token == null) {
                return BadRequest();
            }
            User user = await DAO.PasswordResetToken.GetUser(token);
            if (user == null) {
                return Unauthorized();
            }
            return Ok(user);
        }

        [HttpPost("update/profilepicture")]
        public async Task<ActionResult> UpdateProfilePicture() {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            throw new NotImplementedException();
            /*
            if (!Request.Content.IsMimeMultipartContent()) {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "no file sent");
            }
            byte[] image = (await Utils.GetImages(Request)).FirstOrDefault();
            if (image == null) {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "no file sent");
            }
            await UserRepo.UpdateProfilePicture(image, user);
            return Request.CreateResponse(HttpStatusCode.OK);
            */
        }
    }
}
