using Model;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QWest.Controllers {
    public class ProfileController : Controller {
        public async Task<ActionResult> UserId(int id) {
            User user = await DAO.User.Get(id);
            ViewBag.UserData = user;
            ViewBag.Title = "Profile of " + user.Username;

            return View("Index");
        }
    }
}
