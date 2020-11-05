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
using Utilities;

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

        [HttpGet]
        [ResponseType(typeof(IEnumerable<Group>))]
        public async Task<HttpResponseMessage> FetchUsers(int? userId) {
            int finalId;
            if (userId == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)userId;
            }
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Group.FetchUsers(finalId));
        }

        [Serializable]
        public class UpdateArgument {
            public int id;
            public string description;
            public string name;
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Update([FromBody] UpdateArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null || !await DAO.Group.IsMember(argument.id, user)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await DAO.Group.Update(argument.id, argument.name, argument.description);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
