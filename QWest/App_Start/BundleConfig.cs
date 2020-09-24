using Microsoft.Ajax.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;

namespace QWest {
    public class BundleConfig {
        public static void RegisterBundles(BundleCollection bundles) {
            var scriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts"));
            var scriptsPath = Directory.GetDirectories(scriptPath);

            Task.WaitAll(scriptsPath.Select(async path => { 
                string npmiResult = await Utilities.Utilities.Shell("npm i", path);
                Debug.WriteLine("npm i in " + path);
                Debug.WriteLine(npmiResult);
                string webpackResult = await Utilities.Utilities.Shell("npx webpack --config webpack.config.js", path);
                Debug.WriteLine("npx webpack in " + path);
                Debug.WriteLine(webpackResult);
            }).ToArray());

            var scriptsName = scriptsPath.Select(x => Path.GetFileName(x));
            scriptsName.ForEach(x => {
                bundles.Add(new ScriptBundle("~/scripts/" + x).Include("~/scripts/" + x + "/dist/main.js"));
            });
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
