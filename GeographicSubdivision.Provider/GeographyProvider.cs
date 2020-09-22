using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
            Entities = Countries.Select(Traverse).SelectMany(i => i).Cast<ISubdividable>().Concat(Countries).ToList();
            Dictionary<string, List<ISubdividable>> nameMap = new Dictionary<string, List<ISubdividable>>();
            Entities.ForEach(dividable => {
                dividable.Names.ForEach(name => {
                    if (!nameMap.ContainsKey(name)) {
                        nameMap.Add(name, new List<ISubdividable>());
                    }
                    nameMap[name].Add(dividable);
                });
            });
            NameMap = nameMap;
        }

        public IEnumerable<Subdivision> Traverse(ISubdividable subdividable) {
            return subdividable.Subdivisions.Select(Traverse).SelectMany(i => i);
        }

        public List<Country> Countries { get; }

        public List<ISubdividable> Entities { get; }

        public Dictionary<string, List<ISubdividable>> NameMap {get;}
    }
}
