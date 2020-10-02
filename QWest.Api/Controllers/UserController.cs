using Model;
using QWest.DataAcess;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace QWest.Apis {
    public class UserController : ApiController {
        public async Task<User> Id(int id) {
            User user = await DAO.User.Get(id);
            if(user == null) {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else {
                return user;
            }
        }
    }
}
