using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Tray;
using ssh_tunnel_agent.Windows;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ssh_tunnel_agent.Data {
    public class Session : EditableObject<Session> {
        private TrayViewModel _viewModel;
        private Process _process;
        private bool _processExit;
        private Timer _reconnectTimer;
        private SessionConsole _sessionConsole;

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

        private ObservableCollection<Tunnel> _tunnels = new ObservableCollection<Tunnel>();
        public ObservableCollection<Tunnel> Tunnels {
            set { _tunnels = value; }
            get { return _tunnels; }
        }

        public Session(TrayViewModel viewModel)
            : this() {
            SetViewModel(viewModel);
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
        }

        private string getArguments() {
            string command = String.Empty;
            StringBuilder sb = new StringBuilder("-v -ssh");

            if (Username != String.Empty)
                sb.AppendFormat(" -l {0}", escapeArg(Username));

            sb.AppendFormat(" -P {0}", Port);

            if (Compression)
                sb.Append(" -C");

            sb.Append(X11Forwarding ? " -X" : " -x");
            sb.Append(AgentForwarding ? " -A" : " -a");

            if (!StartShell && !SendCommands)
                sb.Append(" -N");

            if (PrivateKeyFile != String.Empty)
                sb.AppendFormat(" -i {0}", escapeArg(PrivateKeyFile));

            sb.Append(UsePageant ? " -agent" : " -noagent");

            if (SendCommands) {
                if (RemoteCommand != String.Empty)
                    command = RemoteCommand;
                else if (RemoteCommandFile != String.Empty)
                    sb.AppendFormat(" -m {0}", escapeArg(RemoteCommandFile));

                if (RemoteCommandSubsystem)
                    sb.Append(" -s");
            }
            else
                foreach (Tunnel tunnel in Tunnels)
                    sb.Append(tunnel.ToString(" -{0} {1}:{2}", ":{3}:{4}"));

            sb.AppendFormat(" {0}", escapeArg(Host));

            if (command != String.Empty)
                sb.AppendFormat(" {0}", escapeArg(command));

            return sb.ToString();
        }

        private string escapeArg(string arg) {
            arg = arg.Trim();
            return String.Format("{1}{0}{1}", arg.Replace("\"", "\\\""), arg.Contains(" ") ? "\"" : String.Empty);
        }

        public void SetViewModel(TrayViewModel viewModel) {
            this._viewModel = viewModel;
        }


        public TrayViewModel GetViewModel() {
            return _viewModel;
        }

        public void ToggleConnection() {
            if (Status == SessionStatus.CONNECTED)
                Disconnect();
            else
                Connect();
        }

        public void Connect() {
            if (Status == SessionStatus.CONNECTED)
                return;

            Debug.WriteLine("connect: \"" + Name + "\"; " + App.Plink + " " + getArguments());

            _processExit = false;

            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = App.Plink,
                Arguments = getArguments(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            _process = Process.Start(startInfo);
            if (_process == null)
                return;

            _process.EnableRaisingEvents = true;
            _process.Exited += process_Exited;

            if (_sessionConsole != null)
                _sessionConsole.Close();

            _sessionConsole = new SessionConsole(_process, this);
            _sessionConsole.Show();

            Status = SessionStatus.CONNECTED;
        }

        public void Disconnect() {
            if (Status != SessionStatus.CONNECTED)
                return;

            Debug.WriteLine("disconnect: \"" + Name + "\"");

            _processExit = true;
            _process.Kill();
            _process.WaitForExit();
            _process.Close();

            if (_sessionConsole != null)
                _sessionConsole.Close();

            Status = SessionStatus.DISCONNECTED;
        }

        private void process_Exited(object sender, EventArgs e) {
            if (!_processExit) {
                Debug.WriteLine("exit: \"" + Name + "\"; " + _process.ExitCode);

                if (_process.ExitCode == 0) {
                    Status = SessionStatus.DISCONNECTED;

                    if (_sessionConsole != null)
                        _sessionConsole.InvokeClose();
                }
                else {
                    Status = SessionStatus.ERROR;

                    if (AutoReconnect)
                        (_reconnectTimer ?? (
                                _reconnectTimer = new Timer(state => Connect())
                            )).Change(new Random().Next(500, 5000), Timeout.Infinite);
                }

                _process.Close();
            }
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
