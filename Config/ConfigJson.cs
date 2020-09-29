using Newtonsoft.Json;
using System;

namespace Config {
    [Serializable]
    public class ConfigJson {
        [JsonProperty("database_connection_string")]
        private string _databaseConnectionString;

        public string DatabaseConnectionString {
            get {
                return _databaseConnectionString;
            }
        }
    }
}
