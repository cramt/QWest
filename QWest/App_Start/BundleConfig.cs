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

            Task.WaitAll(scriptsPath.Select(path => {
                return Task.Factory.StartNew(() => {
                    //npx webpack --config webpack.config.js
                    Process process = new Process {
                        StartInfo = new ProcessStartInfo {
                            WorkingDirectory = path,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            FileName = @"C:\Windows\System32\cmd.exe",
                            Verb = "runas",
                            Arguments = "/c npx webpack --config webpack.config.js",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    Debug.WriteLine(process.StandardOutput.ReadToEnd());
                });
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
