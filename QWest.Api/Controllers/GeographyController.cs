﻿using Model.Geographic;
using QWest.DataAcess;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class GeographyController : ApiController {
        [ResponseType(typeof(GeopoliticalLocation))]
        public async Task<HttpResponseMessage> Get(string alpha2) {
            alpha2 = alpha2.ToUpper();
            GeopoliticalLocation local = await DAO.Geography.GetAnyByAlpha2s(alpha2);
            if(local == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else {
                return Request.CreateResponse(HttpStatusCode.OK, local);
            }
        }
        [ResponseType(typeof(IEnumerable<Country>))]
        public async Task<HttpResponseMessage> Get() {
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Geography.CreateBackup());
        }
        [ResponseType(typeof(IEnumerable<Country>))]
        public async Task<HttpResponseMessage> GetCountries() {
            return Request.CreateResponse(HttpStatusCode.OK, await DAO.Geography.GetCountries());
        }
        [ResponseType(typeof(IEnumerable<Subdivision>))]
        public async Task<HttpResponseMessage> GetSubdivisions(int superId) {
            var results = await DAO.Geography.GetSubdivisions(superId);
            if(results.Count() == 0) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}