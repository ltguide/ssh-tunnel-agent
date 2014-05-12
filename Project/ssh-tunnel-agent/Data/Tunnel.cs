using ssh_tunnel_agent.Classes;
using System;
using System.Text;

namespace ssh_tunnel_agent.Data {
    public class Tunnel : NotifyPropertyChangedBase {
        public TunnelType Type { get; set; }
        public uint ListenPort { get; set; }
        public string ListenIP { get; set; }
        public string Host { get; set; }
        public uint Port { get; set; }

        public Tunnel() {
            ListenIP = "0.0.0.0";
            Type = TunnelType.LOCAL;
        }

        public string ToString(string basic, string extended) {
            StringBuilder sb = new StringBuilder(basic);
            if (Type != TunnelType.DYNAMIC)
                sb.Append(extended);

            return String.Format(sb.ToString(), Type.getCode(), ListenIP, ListenPort, Host, Port);
        }
    }
}
