using System.Configuration;
using System.Windows;

namespace ssh_tunnel_agent.Config {
    public class Settings {
        private Configuration _config;
        private SettingsSection _settings;

        public Settings() {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            bool addSettings = true;
            try {
                if (_config.Sections["Settings"] != null)
                    addSettings = false;
            }
            catch (ConfigurationErrorsException) {
                _config.Sections.Remove("Settings");
            }

            if (addSettings)
                _config.Sections.Add("Settings", new SettingsSection());

            _settings = (SettingsSection)_config.Sections["Settings"];
        }

        public void SetCData(string key, string value) {
            Get<TextConfigurationElement>(key).Value = value;
        }

        public void Set(string key, object value) {
            typeof(SettingsSection).GetProperty(key).SetValue(_settings, value);
        }

        public string GetCData(string key) {
            return Get<TextConfigurationElement>(key).Value;
        }

        public T Get<T>(string key) {
            return (T)typeof(SettingsSection).GetProperty(key).GetValue(_settings);
        }

        public void Save() {
            try {
                _config.Save(ConfigurationSaveMode.Modified);
            }
            catch (ConfigurationErrorsException ex) {
                MessageBox.Show(ex.Message, "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
