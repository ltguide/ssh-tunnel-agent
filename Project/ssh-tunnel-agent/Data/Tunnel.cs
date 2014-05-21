using Newtonsoft.Json;
using ssh_tunnel_agent.Classes;
using System;
using System.ComponentModel;
using System.Text;

namespace ssh_tunnel_agent.Data {
    public class Tunnel : NotifyPropertyChangedBase, IDataErrorInfo {
        private TunnelType _type;
        public TunnelType Type {
            get { return _type; }
            set {
                _type = value;
                NotifyPropertyChanged();

                if (Type == TunnelType.DYNAMIC) {
                    Host = String.Empty;
                    Port = String.Empty;
                }
            }
        }

        public string ListenPort { get; set; }
        public string ListenIP { get; set; }

        private string _host;
        public string Host {
            get { return _host; }
            set {
                _host = value;
                NotifyPropertyChanged();
            }
        }

        private string _port;
        public string Port {
            get { return _port; }
            set {
                _port = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public string Error {
            get { return ""; }
        }

        public Tunnel() {
            ListenIP = "0.0.0.0";
            Type = TunnelType.LOCAL;
        }

        public override string ToString() {
            return ToString("{0} {1}:{2}", " > {3}:{4}");
        }

        public string ToString(string basic, string extended) {
            StringBuilder sb = new StringBuilder(basic);
            if (Type != TunnelType.DYNAMIC)
                sb.Append(extended);

            return String.Format(sb.ToString(), Type.getCode(), ListenIP, ListenPort, Host, Port);
        }

        public string this[string name] {
            get {
                switch (name) {
                    case "ListenIP":
                        return checkHost(ListenIP);
                    case "ListenPort":
                        return checkPort(ListenPort);
                    case "Host":
                        if (Type != TunnelType.DYNAMIC)
                            return checkHost(Host);
                        break;
                    case "Port":
                        if (Type != TunnelType.DYNAMIC)
                            return checkPort(Port);
                        break;
                }

                return null;
            }
        }

        private string checkHost(string host) {
            if (host != null && host != String.Empty)
                if (host.Contains(" ") || host.Contains("\""))
                    return "Poorly formatted value.";

            return null;
        }

        private string checkPort(string port) {
            if (port != null && port != String.Empty) {
                ushort x;
                if (!IsDigitsOnly(port) || !UInt16.TryParse(port, out x) || x == 0)
                    return "Port must be a positive integer under " + UInt16.MaxValue + ".";
            }

            return null;
        }

        private bool IsDigitsOnly(string str) {
            foreach (char c in str)
                if (c < '0' || c > '9')
                    return false;

            return true;
        }
    }
}
