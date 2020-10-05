using Microsoft.Owin;
using Model;
using QWest.DataAcess;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace QWest.Api {
    public class AuthenticationMiddleware : OwinMiddleware {
        public AuthenticationMiddleware(OwinMiddleware next) : base(next) {
        }
        public override async Task Invoke(IOwinContext context) {
            var cookies = context.Request.Cookies.Where(x => x.Key == "sessionCookie").ToList();
            if (cookies.Count == 0) {
                await Next.Invoke(context);
                return;
            }
            var cookie = cookies[0].Value;
            var value = WebUtility.UrlDecode(cookie);
            if (value == "") {
                await Next.Invoke(context);
                return;
            }
            if (value.First() == '"' && value.Last() == '"') {
                value = value.Substring(1, value.Length - 2);
            }
            User user = await DAO.User.GetBySessionCookie(value);
            if(user == null) {
                await Next.Invoke(context);
                return;
            }
            context.Set("user", user);
            await Next.Invoke(context);
        }
    }
}
