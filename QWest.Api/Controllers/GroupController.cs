using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Api.Controllers {
    public class GroupController : ApiController {
        private DAO.IGroup _groupRepo = null;
        public DAO.IGroup GroupRepo {
            get {
                if (_groupRepo == null) {
                    _groupRepo = DAO.Group;
                }
                return _groupRepo;
            }
            set {
                _groupRepo = value;
            }
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Add([FromBody] Group group) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            if (group.Members.Where(x => x.Id == user.Id).Count() == 0) {
                group.Members.Add(user);
            }
            await DAO.Group.Create(group);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
