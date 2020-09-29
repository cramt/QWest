using Model;
using QWest.DataAcess;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QWest {
    public class Utils {
        public static async Task<User> Authorize(Controller controller) {
            string sessionCookie = controller.Request.Cookies.Get("sessionCookie").Value;
            if (sessionCookie == null) {
                return null;
            }
            sessionCookie = WebUtility.UrlDecode(sessionCookie);
            User user = await DAO.User.GetBySessionCookie(sessionCookie);
            return user;
        }
    }
}