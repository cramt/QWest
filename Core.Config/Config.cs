using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Config {
    public class Config {
        private static ConfigJson _instance;
        public static ConfigJson Instance {
            get {
                if(_instance == null) {
                    string path = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.FullName + "\\Core.Config\\config.json";

                    _instance = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText(path));
                }
                return _instance;
            }
        }
        private Config() {
            
        }
    }
}
