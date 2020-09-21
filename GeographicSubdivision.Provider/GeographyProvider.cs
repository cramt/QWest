using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GeographicSubdivision.Provider {
    public class GeographyProvider {
        private static GeographyProvider _instance;
        public static GeographyProvider Instance {
            get {
                if (_instance == null) {
                    _instance = new GeographyProvider();
                }
                return _instance;
            }
        }
        private GeographyProvider() {
            Countries = JsonConvert.DeserializeObject<List<Country>>(ISO3166String.ISO3166);
            foreach (Country country in Countries) {
                country.SetBackwardsReference();
            }
            EntityNames = Countries.Select(Traverse).SelectMany(i => i).Select(x => x.Name).Concat(Countries.Select(x => x.Name)).ToList();
        }

        public IEnumerable<Subdivision> Traverse(ISubdividable subdividable) {
            return subdividable.Subdivisions.Select(Traverse).SelectMany(i => i);
        }

        public List<Country> Countries { get; }

        public List<string> EntityNames { get; }
    }
}
