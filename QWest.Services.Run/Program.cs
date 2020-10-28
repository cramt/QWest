using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;

namespace QWest.Services.Run {
    class Program {
        static void Main(string[] args) {
            var config = Config.Config.Instance;
            Task.WaitAll(config.Ports.Select(KillOnPort).ToArray());
            var services = Service.GenerateServices((service, x) => {
                Console.WriteLine(service.Name + ": " + x);
            });
            var names = services.Select(x => x.Name).ToList();
            var originalConsole = Console.Out;
            Console.SetOut(new ConsoleCatcher(names, originalConsole));
            Task.WaitAll(services.Select(async x => {
                Console.WriteLine("initializing " + x.Name);
                await x.Run();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(x.Name + " has stopped running");
                Console.ResetColor();
            }).ToArray());
        }

        class ConsoleCatcher : TextWriter {
            private List<string> _names;
            private StringBuilder _buffer = new StringBuilder();
            private TextWriter _original;
            public ConsoleCatcher(List<string> names, TextWriter original) {
                _names = names;
                _original = original;
            }

            public override Encoding Encoding { get { return Encoding.UTF8; } }

            public override void Write(char c) {
                _buffer.Append(c);
                string str = _buffer.ToString();
                if (str.Contains(Environment.NewLine)) {
                    string stackTrace = Environment.StackTrace;
                    string service = _names.Select(x => (x, Regex.Matches(stackTrace, x).Count)).Where(x => x.Count != 0).OrderBy(x => x.Count).Select(x => x.x).FirstOrDefault();
                    string pre = service.MapValue(x => x + ": ").UnwrapOr("");
                    str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x => {
                        _original.WriteLine(pre + x);
                    });
                    _buffer.Clear();
                }
            }
        }
    }
}
