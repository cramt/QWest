using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
            Entities = Countries.Select(Traverse).SelectMany(i => i).Cast<AbstractLocation>().Concat(Countries).ToList();
            Dictionary<string, List<AbstractLocation>> nameMap = new Dictionary<string, List<AbstractLocation>>();
            foreach (AbstractLocation dividable in Entities) {
                dividable.Names.ForEach(name => {
                    if (!nameMap.ContainsKey(name)) {
                        nameMap.Add(name, new List<AbstractLocation>());
                    }
                    nameMap[name].Add(dividable);
                });
            };
            NameMap = nameMap.ToDictionary(x => x.Key, x => x.Value as IReadOnlyCollection<AbstractLocation>);

            Dictionary<string, List<AbstractLocation>> alpha2Map = new Dictionary<string, List<AbstractLocation>>();
            foreach (AbstractLocation dividable in Entities) {
                string id = dividable.GetFullId();
                if (!alpha2Map.ContainsKey(id)) {
                    alpha2Map.Add(id, new List<AbstractLocation>());
                }
                alpha2Map[id].Add(dividable);

            };
            Alpha2Map = alpha2Map.ToDictionary(x => x.Key, x => x.Value as IReadOnlyCollection<AbstractLocation>);
        }

        public IEnumerable<Subdivision> Traverse(AbstractLocation subdividable) {
            return subdividable.Subdivisions.Select(Traverse).SelectMany(i => i);
        }

        public IReadOnlyCollection<Country> Countries { get; }

        public IReadOnlyCollection<AbstractLocation> Entities { get; }

        public Dictionary<string, IReadOnlyCollection<AbstractLocation>> NameMap { get; }

        public Dictionary<string, IReadOnlyCollection<AbstractLocation>> Alpha2Map { get; }
    }
}
