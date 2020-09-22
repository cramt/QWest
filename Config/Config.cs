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
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.json";
                    string text;
                    if (File.Exists(path)) {
                        text = File.ReadAllText(path);
                    }
                    else {
                        text = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Config.config.json")).ReadToEnd();
                    }
                    _instance = JsonConvert.DeserializeObject<ConfigJson>(text);
                }
                return _instance;
            }
        }
        private Config() {
            
        }
    }
}
