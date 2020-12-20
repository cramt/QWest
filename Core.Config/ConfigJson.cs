using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Config {
    [Serializable]
    public class ConfigJson {
        [JsonProperty("database_connection_string", Required = Required.Always)]
        private string _databaseConnectionString;

        [JsonProperty("api_port", Required = Required.Always)]
        private uint _apiPort;

        [JsonProperty("serve_port", Required = Required.Always)]
        private uint _servePort;

        [JsonProperty("start_year", Required = Required.Always)]
        private uint _startYear;

        [JsonProperty("email_port", Required = Required.Always)]
        private uint _emailPort;

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

        public uint StartYear {
            get {
                return _startYear;
            }
        }

        public DateTime StartDate {
            get {
                return new DateTime((int)StartYear, 1, 1);
            }
        }

        public uint EmailPort {
            get {
                return _emailPort;
            }
        }

        public IEnumerable<uint> Ports {
            get {
                yield return ApiPort;
                yield return ServePort;
                yield return EmailPort;
            }
        }
    }
}
