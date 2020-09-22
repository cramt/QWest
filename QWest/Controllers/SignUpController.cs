using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QWest.Controllers {
    public class SignUpController : Controller {
        public ActionResult Index() {
            ViewBag.Title = "Sign up";

            return View();
        }
    }
}
