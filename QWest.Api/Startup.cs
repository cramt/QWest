using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using static Utilities.Utilities;

namespace QWest.Api {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            JsonSerializerSettings defaultSettings = new JsonSerializerSettings {
                Formatting = DebugMode ? Formatting.Indented : Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = defaultSettings;

            app.Use<AuthenticationMiddleware>();
            app.UseWebApi(config);
        }
    }
}
