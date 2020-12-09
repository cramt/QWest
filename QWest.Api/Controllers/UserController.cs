using Model;
using Newtonsoft.Json;
using QWest.Api;
using QWest.DataAccess;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static Utilities.Utilities;

namespace QWest.Apis {
    public class UserController : ApiController {

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

        [ResponseType(typeof(User))]
        public async Task<HttpResponseMessage> Get(int id) {
            User user = await UserRepo.Get(id);
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, user);
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

        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<HttpResponseMessage> Update([FromBody] NewUser newUser) {
            if (newUser == null) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await UserRepo.Update(newUser.UpdateUser(user));
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [ResponseType(typeof(bool))]
        [HttpPost]
        public async Task<HttpResponseMessage> RequestPasswordReset() {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            string token = await DAO.PasswordResetToken.NewToken(user);
            string url = "http://" + Request.RequestUri.Host + ":" + Config.Config.Instance.ServePort + "/password_reset.html?token=" + WebUtility.UrlEncode(token);
            await SendEmail(new EmailArgument(user.Email, "Password Reset", "You have requested a password reset, please go to this link " + url + " to finalize and save your new password"));
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        public class ConfirmPasswordResetArgument {
            public string token;
            public string password;
        }

        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<HttpResponseMessage> ConfirmPasswordReset([FromBody] ConfirmPasswordResetArgument argument) {
            User user = await DAO.PasswordResetToken.GetUser(argument.token);
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            user.NewPassword(argument.password);
            await Task.WhenAll(new Task[] { UserRepo.Update(user), DAO.PasswordResetToken.DeleteToken(argument.token) });
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        [ResponseType(typeof(User))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetByPasswordResetToken(string token) {
            if (token == null) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            User user = await DAO.PasswordResetToken.GetUser(token);
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateProfilePicture() {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            if (!Request.Content.IsMimeMultipartContent()) {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "no file sent");
            }
            byte[] image = (await Utils.GetImages(Request)).FirstOrDefault();
            if (image == null) {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "no file sent");
            }
            await UserRepo.UpdateProfilePicture(image, user);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
