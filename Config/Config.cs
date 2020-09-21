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
        private static ConfigJson _instance;
        public static ConfigJson Instance {
            get {
                if(_instance == null) {
                    string jsonString = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.json");
                    _instance = JsonConvert.DeserializeObject<ConfigJson>(jsonString);
                }
                return _instance;
            }
        }
        private Config() {
            
        }
    }
}
