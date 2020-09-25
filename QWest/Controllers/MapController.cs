using GeographicSubdivision.Provider;
using Model;
using Newtonsoft.Json;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QWest.Controllers {
    public class MapController : Controller {
        public async Task<ActionResult> UserId(int id) {
            User user = await DAO.User.Get(id);
            ViewBag.UserData = user;
            ViewBag.CountryData = JsonConvert.SerializeObject(GeographyProvider.Instance.Countries.ToDictionary(x => x.Alpha2, x => new Dictionary<string, int> {
                {"visited", 0 }
            }));
            ViewBag.Title = "Map of " + user.Username;

            return View("Index");
        }
    }
}
