
using ssh_tunnel_agent.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ssh_tunnel_agent.Data {
    public class Session : EditableObject<Session> {
        private ViewModel viewModel;

        private string _name;
        public string Name {
            get { return _name; }
            set {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private SessionStatus _status;
        public SessionStatus Status {
            get { return _status; }
            set {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public bool AutoConnect { get; set; }
        public bool AutoReconnect { get; set; }
        public bool SendCommands { get; set; }

        //plink -ssh [options] HOST COMMAND
        public string Username { get; set; }                // -l username
        public string Host { get; set; }                    // host
        public uint Port { get; set; }                      // -P port
        //public System.Security.SecureString Password { get; set; } // -pw passwd // http://stackoverflow.com/questions/12657792/how-to-securely-save-username-password-local
        public bool Compression { get; set; }               // -C
        public bool X11Forwarding { get; set; }             // -X -x
        public bool AgentForwarding { get; set; }           // -A -a
        public bool StartShell { get; set; }                // -N
        public string PrivateKeyFile { get; set; }              // -i keyfile
        public bool UsePageant { get; set; }                   // -agent -noagent
        public string RemoteCommand { get; set; }           // command
        public string RemoteCommandFile { get; set; }       // -m file
        public bool RemoteCommandSubsystem { get; set; }    // -s

        private List<Tunnel> _tunnels = new List<Tunnel>();
        public List<Tunnel> Tunnels {
            set { _tunnels = value; }
            get { return _tunnels; }
        }

        public Session(ViewModel viewModel)
            : this() {
            this.viewModel = viewModel;
        }

        public Session() {
            Name = String.Empty;
            Status = SessionStatus.DISCONNECTED;
            AutoConnect = false;
            AutoReconnect = false;
            SendCommands = false;
            Username = String.Empty;
            Host = String.Empty;
            Port = 22;
            Compression = true;
            X11Forwarding = false;
            AgentForwarding = false;
            StartShell = false;
            PrivateKeyFile = String.Empty;
            UsePageant = true;
            SendCommands = false;
            RemoteCommand = String.Empty;
            RemoteCommandFile = String.Empty;
            RemoteCommandSubsystem = false;



                        Host = "itweb";
            //            Tunnels.Add(new Tunnel() { Type = TunnelType.DYNAMIC, ListenIP = "0.0.0.0", ListenPort = 8080 });
            //            Tunnels.Add(new Tunnel() { Type = TunnelType.LOCAL, ListenIP = "0.0.0.0", ListenPort = 4444, Host = "10.5.205.235", Port = 3389 });
        }

        private string getArguments() {
            StringBuilder sb = new StringBuilder("-v -ssh");

            if (Username != String.Empty)
                sb.AppendFormat(" -l {0}", Username);

            sb.AppendFormat(" -P {0}", Port);

            if (Compression)
                sb.Append(" -C");

            sb.Append(X11Forwarding ? " -X" : " -x");
            sb.Append(AgentForwarding ? " -A" : " -a");

            if (!StartShell && !SendCommands)
                sb.Append(" -N");

            if (PrivateKeyFile != String.Empty)
                sb.AppendFormat(" -i {0}", PrivateKeyFile);

            sb.Append(UsePageant ? " -agent" : " -noagent");

            if (SendCommands) {
                if (RemoteCommandFile != String.Empty)
                    sb.AppendFormat(" -m {0}", RemoteCommandFile);

                if (RemoteCommandSubsystem)
                    sb.Append(" -s");
            }
            else
                foreach (Tunnel tunnel in Tunnels)
                    sb.Append(tunnel.ToString(" -{0} {1}:{2}", ":{0}:{1}"));

            sb.AppendFormat(" {0}", Host);

            if (SendCommands && RemoteCommand != String.Empty)
                sb.AppendFormat(" {0}", RemoteCommand);

            return sb.ToString();
        }

        public void ToggleConnection() {
            if (Status == SessionStatus.CONNECTED)
                Disconnect();
            else
                Connect();
        }

        public void Connect() {
            Status = SessionStatus.CONNECTED;
            Debug.WriteLine("connect: " + Name);
            Debug.WriteLine(getArguments());

            viewModel.ConnectedSessions = null;
        }

        public void Disconnect() {
            Status = SessionStatus.DISCONNECTED;
            Debug.WriteLine("disconnect: " + Name);

            viewModel.ConnectedSessions = null;
        }

        internal string TunnelsToString() {
            StringBuilder sb = new StringBuilder(Name);
            foreach (Tunnel tunnel in Tunnels)
                sb.Append(tunnel.ToString("\n{0} {1}:{2}", " > {0}:{1}"));

            return sb.ToString();
        }
    }
}
