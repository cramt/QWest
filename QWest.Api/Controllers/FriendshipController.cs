using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace QWest.Api.Controllers {
    public class FriendshipController : ApiController {

        private DAO.IFriendship _friendshipRepo = null;
        public DAO.IFriendship FriendshipRepo {
            get {
                if (_friendshipRepo == null) {
                    _friendshipRepo = DAO.Friendship;
                }
                return _friendshipRepo;
            }
            set {
                _friendshipRepo = value;
            }
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> AddFriend(int id) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await FriendshipRepo.AddFriendRequest(user, id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [ResponseType(typeof(IEnumerable<User>))]
        public async Task<HttpResponseMessage> GetRequests() {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            return Request.CreateResponse(HttpStatusCode.OK, await FriendshipRepo.GetFriendRequests(user));
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> AcceptFriendRequest(int id) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await FriendshipRepo.AcceptFriendRequest(id, user);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        [ResponseType(typeof(IEnumerable<User>))]
        public async Task<HttpResponseMessage> GetFriends(int? id = null) {
            int finalId;
            if(id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)id;
            }

            return Request.CreateResponse(HttpStatusCode.OK, await FriendshipRepo.GetUsersFriends(finalId));
        }
    }
}
