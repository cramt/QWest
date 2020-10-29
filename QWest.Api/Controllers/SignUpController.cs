using Model;
using QWest.DataAcess;
using System.Threading.Tasks;
using System.Web.Http;

namespace QWest.Apis {
    public class SignUpController : ApiController {

        public DAO.IUser UserRepo { get; set; } = DAO.User;

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
            await UserRepo.Add(user);
            await UserRepo.SetNewSessionCookie(user);
            //TODO: optimize
            return user.SessionCookie;
        }
    }
}
