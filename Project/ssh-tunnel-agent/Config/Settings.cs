using System.Configuration;
using System.Windows;

namespace ssh_tunnel_agent.Config {
    public class Settings {
        private Configuration config;
        private SettingsSection settings;

        public Settings() {
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

        public void SetCData(string key, string value) {
            Get<TextConfigurationElement>(key).Value = value;
        }

        public void Set(string key, object value) {
            typeof(SettingsSection).GetProperty(key).SetValue(settings, value);
        }

        public string GetCData(string key) {
            return Get<TextConfigurationElement>(key).Value;
        }

        public T Get<T>(string key) {
            return (T)typeof(SettingsSection).GetProperty(key).GetValue(settings);
        }

        public void Save() {
            try {
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (ConfigurationErrorsException ex) {
                MessageBox.Show(ex.Message, "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
