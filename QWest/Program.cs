using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QWest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(GeographicSubdivision.Provider.GeographyProvider.Instance.EntityNames.Count);
            Thread.Sleep(-1);
        }
    }
}
