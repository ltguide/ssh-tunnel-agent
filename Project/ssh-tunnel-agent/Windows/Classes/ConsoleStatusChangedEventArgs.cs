using System;

namespace ssh_tunnel_agent.Windows.Classes {
    public class ConsoleStatusChangedEventArgs : EventArgs {
        public ConsoleStatus Status { get; private set; }

        public ConsoleStatusChangedEventArgs(ConsoleStatus status) {
            Status = status;
        }
    }
}
