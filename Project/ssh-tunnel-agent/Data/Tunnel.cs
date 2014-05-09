
using System;
using System.Text;
namespace ssh_tunnel_agent.Data {
    public class Tunnel {
        public TunnelType Type { get; set; }
        public uint ListenPort { get; set; }
        public string ListenIP { get; set; }
        public string Host { get; set; }
        public uint Port { get; set; }

        public Tunnel() {
            ListenIP = "0.0.0.0";
        }

        public string ToString(string basic, string complete) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(basic, Type.getCode(), ListenIP, ListenPort);

            if (Type == TunnelType.DYNAMIC)
                return sb.ToString();

            sb.AppendFormat(complete, Host, Port);
            return sb.ToString();
        }
    }
}
