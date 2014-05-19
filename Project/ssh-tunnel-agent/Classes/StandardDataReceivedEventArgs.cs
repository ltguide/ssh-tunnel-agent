
using System;
namespace ssh_tunnel_agent.Classes {
    class StandardDataReceivedEventArgs : EventArgs {
        public string Data { get; private set; }
        public StandardStreamType Type { get; private set; }

        public StandardDataReceivedEventArgs(StandardStreamType type, string data) {
            Type = type;
            Data = data;
        }
    }

    public enum StandardStreamType {
        OUTPUT,
        ERROR,
        INPUT
    }
}
