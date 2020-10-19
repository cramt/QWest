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
            Task.WaitAll(config.Ports.Select(x => KillOnPort(x)).ToArray());
            Task.WaitAll(Service.GenerateServices((service, x) => {
                Console.WriteLine(service.Name + ": " + x);
            }).Select(x => x.Run()).ToArray());
        }
    }
}
