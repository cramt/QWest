using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Utilities.Utilities;

namespace QWest.Api {
    class Program {
        static void Main(string[] args) {
            Task.WaitAll(Config.Config.Instance.Ports.Select(x => KillOnPort(x)).ToArray());
            string nodeProject = NodeProjectLocation;
            string baseAddress = $"http://localhost:{Config.Config.Instance.ApiPort}/";
            Process nodeProcess = DynamicShell($"npm start {Config.Config.Instance.ServePort} {Config.Config.Instance.ApiPort}", stdout => {
                Console.WriteLine(stdout);
            }, nodeProject);
            AppDomain.CurrentDomain.ProcessExit += (o, e) => {
                nodeProcess.Kill();
            };
            WebApp.Start<Startup>(url: baseAddress);
            Thread.Sleep(-1);
        }
    }
}
