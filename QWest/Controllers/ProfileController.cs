using Model;
using QWest.DataAcess;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QWest.Controllers {
    public class ProfileController : Controller {
        public async Task<ActionResult> Index() {
            User user = await Utils.Authorize(this);
            if (user == null) {
                return new HttpUnauthorizedResult();
            }
            ViewBag.UserData = user;
            ViewBag.Title = "Profile of " + user.Username;

            return View();
        }
        public async Task<ActionResult> UserId(int id) {
            User user = await DAO.User.Get(id);
            if (user == null) {
                return new HttpNotFoundResult();
            }
            ViewBag.UserData = user;
            ViewBag.Title = "Profile of " + user.Username;

            return View("Index");
        }
    }
}
