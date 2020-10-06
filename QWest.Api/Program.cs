using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static Utilities.Utilities;

namespace QWest.Api {
    class Program {
        static void Main(string[] args) {
            KillOnPort(8080).Wait();
            KillOnPort(9000).Wait();
            string nodeProject = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\QWest.Web";
            string baseAddress = "http://localhost:9000/";
            Process nodeProcess = DynamicShell("npm start", stdout => {
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
