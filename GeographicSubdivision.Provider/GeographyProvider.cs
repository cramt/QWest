using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
            Task.WaitAll(Countries.Select(country => country.SetBackwardsReference()).ToArray());
            Entities = Countries.Select(Traverse).SelectMany(i => i).Cast<ISubdividable>().Concat(Countries).ToList();
            Dictionary<string, List<ISubdividable>> nameMap = new Dictionary<string, List<ISubdividable>>();
            foreach (ISubdividable dividable in Entities) {
                dividable.Names.ForEach(name => {
                    if (!nameMap.ContainsKey(name)) {
                        nameMap.Add(name, new List<ISubdividable>());
                    }
                    nameMap[name].Add(dividable);
                });
            };
            NameMap = nameMap.ToDictionary(x => x.Key, x => x.Value as IReadOnlyCollection<ISubdividable>);
        }

        public IEnumerable<Subdivision> Traverse(ISubdividable subdividable) {
            return subdividable.Subdivisions.Select(Traverse).SelectMany(i => i);
        }

        public IReadOnlyCollection<Country> Countries { get; }

        public IReadOnlyCollection<ISubdividable> Entities { get; }

        public Dictionary<string, IReadOnlyCollection<ISubdividable>> NameMap { get; }
    }
}
