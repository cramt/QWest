using Model.Geographic;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class GeographyController : ApiController {

        private DAO.IGeography _geographyRepo = null;
        public DAO.IGeography GeographyRepo {
            get {
                if (_geographyRepo == null) {
                    _geographyRepo = DAO.Geography;
                }
                return _geographyRepo;
            }
            set {
                _geographyRepo = value;
            }
        }

        [ResponseType(typeof(GeopoliticalLocation))]
        public async Task<HttpResponseMessage> Get(string alpha2) {
            alpha2 = alpha2.ToUpper();
            GeopoliticalLocation local = await GeographyRepo.GetAnyByAlpha2s(alpha2);
            if (local == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, local);
            }
        }
        [ResponseType(typeof(IEnumerable<Country>))]
        public async Task<HttpResponseMessage> Get() {
            return Request.CreateResponse(HttpStatusCode.OK, await GeographyRepo.FetchEverythingParsed());
        }
        [ResponseType(typeof(IEnumerable<Country>))]
        public async Task<HttpResponseMessage> GetCountries() {
            return Request.CreateResponse(HttpStatusCode.OK, await GeographyRepo.GetCountries());
        }
        [ResponseType(typeof(IEnumerable<Subdivision>))]
        public async Task<HttpResponseMessage> GetSubdivisions(int superId) {
            var results = await GeographyRepo.GetSubdivisions(superId);
            if (results.Count() == 0) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
        [HttpGet]
        [ResponseType(typeof(IEnumerable<GeopoliticalLocation>))]
        public async Task<HttpResponseMessage> NameSearch(string searchTerm) {
            return Request.CreateResponse(HttpStatusCode.OK, await GeographyRepo.NameSearch(searchTerm));
        }
    }
}
