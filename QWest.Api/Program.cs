using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Utilities.Utilities;

namespace QWest.Api {
    public class Program {
        public static async Task Run() {
            string baseAddress = $"http://localhost:{Config.Config.Instance.ApiPort}/";
            WebApp.Start<Startup>(url: baseAddress);
            Thread.Sleep(-1);
        }
    }
}
