using Model;
using QWest.DataAcess;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace QWest {
    public class Utils {
        public static async Task<User> Authorize(Controller controller) {
            HttpCookie sessionCookie = controller.Request.Cookies.Get("sessionCookie");
            if (sessionCookie == null) {
                return null;
            }
            string value = sessionCookie.Value;
            if (value == null) {
                return null;
            }
            value = WebUtility.UrlDecode(value);
            if (value.IsEmpty()) {
                return null;
            }
            if(value.First() == '"' && value.Last() == '"') {
                value = value.Substring(1, value.Length - 2);
            }
            User user = await DAO.User.GetBySessionCookie(value);
            return user;
        }
    }
}