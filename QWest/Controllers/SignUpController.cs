using System.Web.Mvc;

namespace QWest.Controllers {
    public class SignUpController : Controller {
        public ActionResult Index() {
            ViewBag.Title = "Sign up";

            return View();
        }
    }
}
