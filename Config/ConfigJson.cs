using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Config {
    [Serializable]
    public class ConfigJson {
        [JsonProperty("database_connection_string")]
        private string _databaseConnectionString;

        [JsonProperty("api_port")]
        private uint _apiPort;

        [JsonProperty("serve_port")]
        private uint _servePort;

        [JsonProperty("gmail_username")]
        private string _gmailUsername;

        [JsonProperty("gmail_password")]
        private string _gmailPassword;

        public string DatabaseConnectionString {
            get {
                return _databaseConnectionString;
            }
        }

        public uint ApiPort {
            get {
                return _apiPort;
            }
        }

        public uint ServePort {
            get {
                return _servePort;
            }
        }

        public string GmailUsername {
            get {
                return _gmailUsername;
            }
        }

        public string GmailPassword {
            get {
                return _gmailPassword;
            }
        }

        public IEnumerable<uint> Ports {
            get {
                yield return ApiPort;
                yield return ServePort;
            }
        }
    }
}
