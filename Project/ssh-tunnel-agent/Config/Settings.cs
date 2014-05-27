using System;
using System.Configuration;
using System.IO;

namespace ssh_tunnel_agent.Config {
    public class Settings {
        private Configuration _config;
        private SettingsSection _settings;

        public Settings(ConfigurationUserLevel userLevel) {
            string file = AppDomain.CurrentDomain.FriendlyName + ".config";

            ExeConfigurationFileMap exeMap = new ExeConfigurationFileMap();
            exeMap.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            exeMap.RoamingUserConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ssh-tunnel-agent", file);
            exeMap.LocalUserConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ssh-tunnel-agent", file);

            _config = ConfigurationManager.OpenMappedExeConfiguration(exeMap, userLevel);

            bool addSettings = true;
            try {
                if (_config.Sections["Settings"] != null)
                    addSettings = false;
            }
            catch (ConfigurationErrorsException) {
                _config.Sections.Remove("Settings");
            }

            if (addSettings) {
                SettingsSection section = new SettingsSection();
                section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;

                _config.Sections.Add("Settings", section);
            }

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

        public string Save() {
            try {
                _config.Save(ConfigurationSaveMode.Modified);
            }
            catch (ConfigurationErrorsException ex) {
                return ex.Message;
            }

            return null;
        }
    }
}
