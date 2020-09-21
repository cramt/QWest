using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace QWest.DataAcess {
    public class ConnectionWrapper {
        private static SqlConnection _instance;
        public static SqlConnection Instance {
            get {
                if(_instance == null) {
                    _instance = new SqlConnection(Config.Config.Instance.ConfigJson.DatabaseConnectionString);
                }
                return _instance;
            }
        }
        private ConnectionWrapper() {

        }
    }
}
