using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static Utilities.Utilities;

namespace QWest.Services.Run {
    public abstract class Service {
        public static IEnumerable<Service> GenerateServices(Action<Service, string> log) {
            yield return new ApiService(log);
        }

        private Action<Service, string> _log;

        public Service(Action<Service, string> log) {
            _log = log;
        }

        protected void Log(string str) {
            _log(this, str);
        }

        public abstract string Name { get; }

        public abstract Task Run();
    }

    public abstract class NodeService : Service {
        public NodeService(Action<Service, string> log) : base(log) {

        }
        public override Task Run() {
            return Task.Factory.StartNew(() => DynamicShell("npm start", Log, SolutionLocation + "\\" + Name).WaitForExit());
        }
    }

    public class ApiService : Service {
        public ApiService(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Api";
            }
        }

        public override Task Run() {
            return Api.Program.Run();
        }
    }

    public class WebSerivce : NodeService {
        public WebSerivce(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Web";
            }
        }
    }

    public class EmailSerivce : NodeService {
        public EmailSerivce(Action<Service, string> log) : base(log) {

        }
        public override string Name {
            get {
                return "QWest.Email";
            }
        }
    }
}
