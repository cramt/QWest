using Model;
using QWest.DataAccess;
using System.Threading.Tasks;
using System.Web.Http;

namespace QWest.Apis {
    public class SignUpController : ApiController {

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
