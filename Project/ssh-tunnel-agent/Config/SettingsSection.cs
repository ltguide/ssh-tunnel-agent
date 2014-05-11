using System.Configuration;

namespace ssh_tunnel_agent.Config {
    public class SettingsSection : ConfigurationSection {
        public SettingsSection() { }

        [ConfigurationProperty("Sessions")]
        public TextConfigurationElement Sessions {
            get { return (TextConfigurationElement)(this["Sessions"]); }
            set { this["Sessions"] = value; }
        }

        [ConfigurationProperty("AutoStartApplication")]
        public bool AutoStartApplication {
            get { return (bool)(this["AutoStartApplication"]); }
            set { this["AutoStartApplication"] = value; }
        }
    }
}
