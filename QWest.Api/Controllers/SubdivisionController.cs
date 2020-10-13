using GeographicSubdivision.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace QWest.Apis {
    public class SubdivisionController : ApiController {
        [ResponseType(typeof(AbstractLocation))]
        public HttpResponseMessage Get(string alpha2) {
            alpha2 = alpha2.ToUpper();
            var map = GeographyProvider.Instance.Alpha2Map;
            if (map.ContainsKey(alpha2)) {
                var super = map[alpha2].FirstOrDefault();
                if (super == null) {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, super);
            }
            else {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
        [ResponseType(typeof(IEnumerable<Country>))]
        public HttpResponseMessage Get() {
            return Request.CreateResponse(HttpStatusCode.OK, GeographyProvider.Instance.Countries);
        }
    }
}
