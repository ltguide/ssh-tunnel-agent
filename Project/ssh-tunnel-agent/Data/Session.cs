using ssh_tunnel_agent.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ssh_tunnel_agent.Data {
    public class Session : EditableObject<Session> {
        private ViewModel viewModel;

        private SessionStatus _status;
        public SessionStatus Status {
            get { return _status; }
            set {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name {
            get { return _name; }
            set {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        //plink -ssh [options] HOST COMMAND
        public string Host { get; set; }                    // host
        public uint Port { get; set; }                      // -P port
        public string Username { get; set; }                // -l username
        //public System.Security.SecureString Password { get; set; } // -pw passwd // http://stackoverflow.com/questions/12657792/how-to-securely-save-username-password-local
        public bool Compression { get; set; }               // -C
        public bool UsePageant { get; set; }                // -agent -noagent
        public bool AgentForwarding { get; set; }           // -A -a
        public string PrivateKeyFile { get; set; }          // -i keyfile

        private bool _sendCommands;
        public bool SendCommands {
            get { return _sendCommands; }
            set {
                _sendCommands = value;
                NotifyPropertyChanged();
            }
        }

        public bool AutoConnect { get; set; }
        public bool AutoReconnect { get; set; }
        public bool StartShell { get; set; }                // -N
        public bool X11Forwarding { get; set; }             // -X -x

        private string _remoteCommand;
        public string RemoteCommand {                       // command
            get { return _remoteCommand; }
            set {
                _remoteCommand = value;
                NotifyPropertyChanged();
            }
        }

        private string _remoteCommandFile;
        public string RemoteCommandFile {                   // -m file
            get { return _remoteCommandFile; }
            set {
                _remoteCommandFile = value;
                NotifyPropertyChanged();
            }
        }
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
            Status = SessionStatus.DISCONNECTED;

            Name = String.Empty;
            Host = String.Empty;
            Port = 22;
            Username = String.Empty;
            Compression = true;
            UsePageant = true;
            AgentForwarding = false;
            PrivateKeyFile = String.Empty;
            SendCommands = false;

            AutoConnect = false;
            AutoReconnect = false;
            StartShell = false;
            X11Forwarding = false;

            RemoteCommand = String.Empty;
            RemoteCommandFile = String.Empty;
            RemoteCommandSubsystem = false;



            //Host = "itweb";
            Tunnels.Add(new Tunnel() { Type = TunnelType.DYNAMIC, ListenIP = "0.0.0.0", ListenPort = 8080 });
            Tunnels.Add(new Tunnel() { Type = TunnelType.LOCAL, ListenIP = "0.0.0.0", ListenPort = 4444, Host = "10.5.205.235", Port = 3389 });
            Tunnels.Add(new Tunnel() { Type = TunnelType.DYNAMIC, ListenIP = "0.0.0.0", ListenPort = 8081 });
            Tunnels.Add(new Tunnel() { Type = TunnelType.DYNAMIC, ListenIP = "0.0.0.0", ListenPort = 8082 });
        }

        private string getArguments() {
            string command = String.Empty;
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
                if (RemoteCommand != String.Empty)
                    command = RemoteCommand;
                else if (RemoteCommandFile != String.Empty)
                    sb.AppendFormat(" -m {0}", RemoteCommandFile);

                if (RemoteCommandSubsystem)
                    sb.Append(" -s");
            }
            else
                foreach (Tunnel tunnel in Tunnels)
                    sb.Append(tunnel.ToString(" -{0} {1}:{2}", ":{3}:{4}"));

            sb.AppendFormat(" {0}", Host);

            if (command != String.Empty)
                sb.AppendFormat(" {0}", command);

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

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name);

            foreach (Tunnel tunnel in Tunnels)
                sb.AppendLine(tunnel.ToString());

            return sb.ToString();
        }
    }
}
