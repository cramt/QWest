using Model;
using QWest.DataAcess;
using System.Threading.Tasks;
using System.Web.Http;

namespace QWest.Apis {
    public class SignUpController : ApiController {
        public class RegisterArgument {
            public string email;
            public string password;
            public string username;

            public User ToUser() {
                return new User(username, password, email);
            }
        }

        [HttpPost]
        public async Task<string> Register([FromBody] RegisterArgument argument) {
            User user = argument.ToUser();
            await DAO.User.Add(user);
            await DAO.User.SetNewSessionCookie(user);
            //TODO: optimize
            return user.SessionCookie;
        }
    }
}
