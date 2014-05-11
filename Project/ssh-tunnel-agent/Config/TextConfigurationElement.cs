using System.Configuration;
using System.Xml;

namespace ssh_tunnel_agent.Config {
    public class TextConfigurationElement : ConfigurationElement {

        [ConfigurationProperty("Value", IsRequired = true, IsKey = true)]
        public string Value {
            get { return (string)this["Value"]; }
            set { this["Value"] = value; }
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            if (writer != null)
                writer.WriteCData(Value);

            return true;
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey) {
            Value = reader.ReadElementContentAsString().Trim();
        }
    }
}
