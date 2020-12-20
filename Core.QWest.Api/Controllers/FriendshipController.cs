using Microsoft.AspNetCore.Mvc;
using Model;
using QWest.DataAccess;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipController : ControllerBase {

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

        [HttpPost("add/{id}")]
        public async Task<ActionResult<bool>> AddFriend(int id) {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            return Ok(await FriendshipRepo.AddFriendRequest(user, id));
        }

        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<User>>> GetRequests() {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            return Ok(await FriendshipRepo.GetFriendRequests(user));
        }

        [HttpPost("accept/{id}")]
        public async Task<ActionResult<bool>> AcceptFriendRequest(int id) {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            return Ok(await FriendshipRepo.AcceptFriendRequest(id, user));
        }

        [HttpGet("friends")]
        public async Task<ActionResult<IEnumerable<User>>> GetFriends(int? id = null) {
            int finalId;
            if(id == null) {
                User user = Request.GetUser();
                if (user == null) {
                    return Unauthorized();
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)id;
            }

            return Ok(await FriendshipRepo.GetUsersFriends(finalId));
        }
    }
}
