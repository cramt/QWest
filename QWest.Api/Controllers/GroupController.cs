﻿using Model;
using QWest.DataAccess;
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
        [Serializable]
        public class AddArgument {
            public string name;
            public string description;
            public List<int> members;
        }

        [HttpPost]
        [ResponseType(typeof(int))]
        public async Task<HttpResponseMessage> Add([FromBody] AddArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            if (argument.members.Where(x => x == user.Id).Count() == 0) {
                argument.members.Add((int)user.Id);
            }
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Group.Create(argument.name, argument.description, argument.members));
        }

        [HttpGet]
        [ResponseType(typeof(IEnumerable<Group>))]
        public async Task<HttpResponseMessage> FetchUsersGroups(int? userId) {
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
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Group.FetchUsersGroups(finalId));
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

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Update([FromBody] UpdateArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null || !await DAO.Group.IsMember(argument.id, user)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            Group group = await GroupRepo.Get(argument.id);
            await GroupRepo.Update(argument.UpdateGroup(group));
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Serializable]
        public class UpdateMembersArgument {
            public int id;
            public List<int> additions;
            public List<int> subtractions;
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> UpdateMembers([FromBody] UpdateMembersArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            if (user == null || !await GroupRepo.IsMember(argument.id, user)) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await GroupRepo.UpdateMembers(argument.id, argument.additions, argument.subtractions);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [ResponseType(typeof(Group))]
        public async Task<HttpResponseMessage> Get(int id) {
            Group group = await GroupRepo.Get(id);
            if (group == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, group);
        }
    }
}
