using Microsoft.Owin.Hosting;
using System;
using System.IO;
using System.Threading;

namespace QWest.Api {
    class Program {
        static void Main(string[] args) {
            string nodeProject = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\QWest.Web";
            string baseAddress = "http://localhost:9000/";
            Utilities.Utilities.DynamicShell("npm start", stdout => {
                Console.WriteLine(stdout);
            }, nodeProject);    
            WebApp.Start<Startup>(url: baseAddress);
            Thread.Sleep(-1);
        }
    }
}
