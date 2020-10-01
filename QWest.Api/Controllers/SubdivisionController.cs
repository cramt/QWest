using GeographicSubdivision.Provider;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace QWest.Apis {
    public class SubdivisionController : ApiController {
        public List<Subdivision> Get(string id) {
            var map = GeographyProvider.Instance.Alpha2Map;
            if (map.ContainsKey(id)) {
                var super = map[id].FirstOrDefault();
                if(super == null) {
                    return null;
                }
                return super.Subdivisions;
            }
            else {
                return null;
            }
        }
    }
}
