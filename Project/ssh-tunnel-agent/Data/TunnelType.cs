
namespace ssh_tunnel_agent.Data {
    public enum TunnelType {
        DYNAMIC,
        LOCAL,
        REMOTE
    }

    public static partial class Extensions {
        public static string getCode(this TunnelType type) {
            return type.ToString().Substring(0, 1);
        }
    }
}
