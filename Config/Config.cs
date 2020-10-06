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
                    string path1 = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName + "\\Config\\config.json";
                    string path2 = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\config.json";
                    Console.WriteLine(path1);
                    string text;
                    if (File.Exists(path1)) {
                        text = File.ReadAllText(path1);
                    }
                    else if (File.Exists(path2)) {
                        text = File.ReadAllText(path2);
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
