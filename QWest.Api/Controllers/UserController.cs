using Model;
using QWest.Api;
using QWest.DataAcess;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static Utilities.Utilities;

namespace QWest.Apis {
    public class UserController : ApiController {
        [ResponseType(typeof(User))]
        public async Task<HttpResponseMessage> Get(int id) {
            User user = await DAO.User.Get(id);
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, user);
            }
        }

        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<HttpResponseMessage> Update([FromBody] User newUser) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null || user.Id != newUser.Id) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await DAO.User.Update(newUser);
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
            string url = "http://" + Request.RequestUri.Host + ":" + Config.Config.Instance.ServePort + "/password_reset.html?token=" + token;
            EmailServer.Server.Instance.SendMail(user.Email, "Password Reset", "You have requested a password reset, please go to this link " + url + " to finalize and save your new password");
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
            await Task.WhenAll(new Task[] { DAO.User.Update(user), DAO.PasswordResetToken.DeleteToken(argument.token) });
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        [ResponseType(typeof(User))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetByPasswordResetToken([FromBody] string token) {
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
            await DAO.User.UpdateProfilePicture(image, user);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
