using GeographicSubdivision.Provider;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace QWest.Apis {
    public class SubdivisionController : ApiController {
        public AbstractLocation Get(string alpha2) {
            alpha2 = alpha2.ToUpper();
            var map = GeographyProvider.Instance.Alpha2Map;
            if (map.ContainsKey(alpha2)) {
                var super = map[alpha2].FirstOrDefault();
                if(super == null) {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return super;
            }
            else {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
        public IEnumerable<Country> Get() {
            return GeographyProvider.Instance.Countries;
        }
    }
}
