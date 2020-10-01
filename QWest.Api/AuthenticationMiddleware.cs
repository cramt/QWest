﻿using Microsoft.Owin;
using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QWest.Api {
    public class AuthenticationMiddleware : OwinMiddleware {
        public AuthenticationMiddleware(OwinMiddleware next) : base(next) {
        }
        public override async Task Invoke(IOwinContext context) {
            var cookies = context.Request.Cookies.Where(x => x.Key == "sessionCookie").ToList();
            if (cookies.Count == 0) {
                return;
            }
            var cookie = cookies[0].Value;
            var value = WebUtility.UrlDecode(cookie);
            if (value == "") {
                return;
            }
            if (value.First() == '"' && value.Last() == '"') {
                value = value.Substring(1, value.Length - 2);
            }
            User user = await DAO.User.GetBySessionCookie(value);
            if(user == null) {
                return;
            }
            context.Set("user", user);
        }
    }
}
