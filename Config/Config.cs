using Newtonsoft.Json;
using System.IO;
using System.Reflection;

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
