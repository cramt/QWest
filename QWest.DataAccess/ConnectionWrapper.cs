using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using DbUp;
using System.Reflection;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using System.IO;

namespace QWest.DataAcess {
    public class ConnectionWrapper {
        private static SqlConnection _instance;
        public static void ResetInstance() {
            _instance = null;
        }
        public static SqlConnection Instance {
            get {
                if (_instance == null) {
                    _instance = CreateConnection();
                }
                return _instance;
            }
        }

        public class ScriptProvider : IScriptProvider {
            public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager) {
                return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Where(name => name.EndsWith(".sql"))
                    .Select(name => {
                        Console.WriteLine(name);
                        Stack<string> stack = new Stack<string>(name.Split('.'));
                        stack.Pop();
                        int? res = null;
                        if (int.TryParse(stack.Pop(), out int i)) {
                            res = i;
                        }
                        return new Tuple<string, int?>(name, res);
                    })
                    .Where(x => x.Item2 != null)
                    .OrderBy(x => x.Item2)
                    .Select(x => x.Item1)
                    .Select(name => {
                        string text = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)).ReadToEnd();
                        return new Tuple<string, string>(name, text);
                    })
                    .Select(x => new SqlScript(x.Item1, x.Item2));
            }
        }

        private static SqlConnection CreateConnection() {
            string connString = Config.Config.Instance.DatabaseConnectionString;
            DatabaseUpgradeResult result = DeployChanges.To
                .SqlDatabase(connString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build().PerformUpgrade();
            if (!result.Successful) {
                throw result.Error;
            }
            var conn = new SqlConnection(connString);
            conn.Open();
            return conn;
        }
    }
}
