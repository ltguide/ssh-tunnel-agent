using System.ComponentModel;
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


        public static void Set(string key, object value) {
            Settings.Remove(key);
            instance.config.AppSettings.Settings.Add(key, value.ToString());
        }

        public static void Remove(string key) {
            instance.config.AppSettings.Settings.Remove(key);
        }

        public static string Get(string key) {
            try {
                return instance.config.AppSettings.Settings[key].Value;
            }
            catch {
                return null;
            }
        }

        public static T Get<T>(string key) {
            string value = Get(key);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

            if (value == null || converter == null)
                return default(T);

            return (T)converter.ConvertFrom(value);
        }

        public static void Save() {
            instance.config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
