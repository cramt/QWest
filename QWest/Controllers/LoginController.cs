using Model;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QWest.Controllers {
    public class LoginController : Controller {
        public async Task<ActionResult> Index() {
            User user = await Utils.Authorize(this);
            if(user != null) {
                return Redirect("/Profile");
            }
            ViewBag.Title = "Login";

            return View();
        }
    }
}
