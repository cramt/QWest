using Microsoft.AspNetCore.Http;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.Api {
    public static class Utils {
        public static User GetUser(this HttpRequest request) {
            var items = request.HttpContext.Items;
            if (items.ContainsKey("user")) {
                return (User)items["user"];
            }
            return null;
        }
    }
}
