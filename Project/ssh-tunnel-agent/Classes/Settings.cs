using System;
using System.Configuration;

namespace ssh_tunnel_agent {
    public sealed class Settings {
        private static readonly Settings instance = new Settings();
        Configuration config;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Settings() { }

        private Settings() {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static Settings Instance {
            get { return instance; }
        }


        public static void Add(string key, string value) {
            Settings.Remove(key);
            instance.config.AppSettings.Settings.Add(key, value);
        }

        public static void Remove(string key) {
            instance.config.AppSettings.Settings.Remove(key);
        }

        public static string Get(string key) {
            try {
                return instance.config.AppSettings.Settings[key].ToString();
            }
            catch {
                return null;
            }
        }

        public static void Save() {
            instance.config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
