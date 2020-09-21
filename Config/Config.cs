using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Config {
    public class Config {
        private static Config _instance;
        public static Config Instance {
            get {
                if(_instance == null) {
                    _instance = new Config();
                }
                return _instance;
            }
        }
        private Config() {
            string jsonString = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.json");
            ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(jsonString);
        }
        public ConfigJson ConfigJson { get; }
    }
}
