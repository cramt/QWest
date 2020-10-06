﻿using Model;
using QWest.DataAcess;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace QWest.Apis {
    public class UserController : ApiController {
        public async Task<User> Id(int id) {
            User user = await DAO.User.Get(id);
            user.SessionCookie = null;
            if(user == null) {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else {
                return user;
            }
        }
    }
}
