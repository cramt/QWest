using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using DbUp;
using System.Reflection;
using DbUp.Engine;

namespace QWest.DataAcess {
    public class ConnectionWrapper {
        private static SqlConnection _instance;
        public static SqlConnection Instance {
            get {
                if(_instance == null) {
                    _instance = CreateConnection();
                }
                return _instance;
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
            return conn;
        }
    }
}
