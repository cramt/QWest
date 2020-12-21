using Microsoft.AspNetCore.Mvc;
using Model;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase {
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
        [Serializable]
        public class AddArgument {
            public string name;
            public string description;
            public List<int> members;
        }

        [HttpPost("add")]
        public async Task<ActionResult<int>> Add([FromBody] AddArgument argument) {
            User user = Request.GetUser();
            if (user == null) {
                return Unauthorized();
            }
            if (argument.members.Where(x => x == user.Id).Count() == 0) {
                argument.members.Add((int)user.Id);
            }
            return Ok(await DAO.Group.Create(argument.name, argument.description, argument.members));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Group>>> FetchUsersGroups(int? userId) {
            int finalId;
            if (userId == null) {
                User user = Request.GetUser();
                if (user == null) {
                    return Unauthorized();
                }
                finalId = (int)user.Id;
            }
            else {
                finalId = (int)userId;
            }
            return Ok(await DAO.Group.FetchUsersGroups(finalId));
        }

        [Serializable]
        public class UpdateArgument {
            public int id;
            public string description;
            public string name;

            public Group UpdateGroup(Group group)
            {
                if (description != null)
                {
                    group.Description = description;
                }
                if (name != null)
                {
                    group.Name = name;
                }

                return group;
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update([FromBody] UpdateArgument argument) {
            User user = Request.GetUser();
            if (user == null || !await DAO.Group.IsMember(argument.id, user)) {
                return Unauthorized();
            }
            Group group = await GroupRepo.Get(argument.id);
            await GroupRepo.Update(argument.UpdateGroup(group));
            return Ok();
        }

        [Serializable]
        public class UpdateMembersArgument {
            public int id;
            public List<int> additions;
            public List<int> subtractions;
        }

        [HttpPost("update/members")]
        public async Task<ActionResult> UpdateMembers([FromBody] UpdateMembersArgument argument) {
            User user = Request.GetUser();
            if (user == null || !await GroupRepo.IsMember(argument.id, user)) {
                return Unauthorized();
            }
            await GroupRepo.UpdateMembers(argument.id, argument.additions, argument.subtractions);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> Get(int id) {
            Group group = await GroupRepo.Get(id);
            if (group == null) {
                return NotFound();
            }
            return Ok(group);
        }
    }
}
