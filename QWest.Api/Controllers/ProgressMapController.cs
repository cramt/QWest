﻿using Model;
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

namespace QWest.Api.Controllers {
    public class ProgressMapController : ApiController {

        public DAO.IProgressMap ProgressMapRepo { get; set; } = DAO.ProgressMap;

        [HttpGet]
        [ResponseType(typeof(ProgressMap))]
        public Task<HttpResponseMessage> UserId() {
            return UserId(null);
        }
        [HttpGet]
        [ResponseType(typeof(ProgressMap))]
        public async Task<HttpResponseMessage> UserId(int? id) {
            if (id == null) {
                User user = Request.GetOwinContext().Get<User>("user");
                if (user == null) {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                return Request.CreateResponse(HttpStatusCode.OK, await ProgressMapRepo.Get(user));
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, await ProgressMapRepo.GetByUserId((int)id));
            }
        }
        public class UpdateArgument {
            public int id;
            public List<int> additions;
            public List<int> subtractions;
        }
        [HttpPost]
        [ResponseType(typeof(bool))]
        public async Task<HttpResponseMessage> Change([FromBody] UpdateArgument argument) {
            User user = Request.GetOwinContext().Get<User>("user");
            ProgressMap map = await ProgressMapRepo.Get(user);
            if(argument.id != map.Id) {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            await ProgressMapRepo.Update(argument.id, argument.additions, argument.subtractions);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}
