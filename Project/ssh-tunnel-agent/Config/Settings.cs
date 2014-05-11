using System.Configuration;

namespace ssh_tunnel_agent.Config {
    public sealed class Settings {
        private static readonly Settings instance = new Settings();
        Configuration config;
        SettingsSection settings;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Settings() { }

        private Settings() {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            bool addSettings = true;
            try {
                if (config.Sections["Settings"] != null)
                    addSettings = false;
            }
            catch (ConfigurationErrorsException) {
                config.Sections.Remove("Settings");
            }

            if (addSettings)
                config.Sections.Add("Settings", new SettingsSection());

            settings = (SettingsSection)config.Sections["Settings"];
        }

        public static Settings Instance {
            get { return instance; }
        }

        public static void SetCData(string key, string value) {
            Get<TextConfigurationElement>(key).Value = value;
        }

        public static void Set(string key, object value) {
            typeof(SettingsSection).GetProperty(key).SetValue(instance.settings, value);
        }

        public static string GetCData(string key) {
            return Get<TextConfigurationElement>(key).Value;
        }

        public static T Get<T>(string key) {
            return (T)typeof(SettingsSection).GetProperty(key).GetValue(instance.settings);
        }

        public static void Save() {
            instance.config.Save(ConfigurationSaveMode.Modified);
        }

        /*//AppSettings
        public static string Get(string key) {
            KeyValueConfigurationElement kvp = instance.config.AppSettings.Settings[key];
            if (kvp == null)
                return null;

            return kvp.Value;
        }

        public static T Get<T>(string key) {
            string value = Get(key);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

            if (value == null || converter == null)
                return default(T);

            return (T)converter.ConvertFrom(value);
        }*/
    }
}
