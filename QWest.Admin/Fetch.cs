using Model.Geographic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace QWest.Admin {
    public static class Fetch {
        public static async Task<IEnumerable<Country>> GetCountries() {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"http://localhost:{Config.Config.Instance.ApiPort}/api/Geography/GetCountries");
            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Country>>(jsonString);
        }
        public static async Task<List<Subdivision>> GetSubdivisions(int superId) {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"http://localhost:{Config.Config.Instance.ApiPort}/api/Geography/GetSubdivisions?superId={superId}");
            if(response.StatusCode == HttpStatusCode.NotFound) {
                return null;
            }
            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Subdivision>>(jsonString);
        }
        public static async Task<List<Subdivision>> GetSubdivisions(GeopoliticalLocation location) {
            var results = (await GetSubdivisions((int)location.Id));
            location.Subdivisions = results;
            return results;
        }
    }
}
