using System;

namespace ssh_tunnel_agent.Windows.Classes {
    class StandardDataReceivedEventArgs : EventArgs {
        public char[] Data { get; private set; }
        public StandardStreamType Type { get; private set; }
        public string Value { get; private set; }

        public StandardDataReceivedEventArgs(StandardStreamType type, char[] data) {
            Type = type;
            Data = data;
            Value = new string(data);
        }
    }
}
