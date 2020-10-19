using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.Utilities;

namespace QWest.Services.Run {
    class Program {
        static void Main(string[] args) {
            var config = Config.Config.Instance;
            Task.WaitAll(config.Ports.Select(KillOnPort).ToArray());
            var services = Service.GenerateServices((service, x) => {
                Console.WriteLine(service.Name + ": " + x);
            });
            Task.WaitAll(services.Select(x => {
                Console.WriteLine("initializing " + x.Name);
                return x.Run();
            }).ToArray());
        }
    }
}
