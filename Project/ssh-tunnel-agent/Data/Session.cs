using ssh_tunnel_agent.Classes;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ssh_tunnel_agent.Data {
    public class Session : EditableObject<Session> {
        private ViewModel viewModel;
        private Process process;
        private bool processExit;
        private Timer reconnectTimer;
        private event EventHandler<StandardDataReceivedEventArgs> StandardDataReceived;
        private CancellationTokenSource tokenSource;

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

        public Session(ViewModel viewModel)
            : this() {
            SetViewModel(viewModel);
        }

        public Session() {
            StandardDataReceived += session_StandardDataReceived;

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
            //Tunnels.Add(new Tunnel() { Type = TunnelType.LOCAL, ListenIP = "0.0.0.0", ListenPort = 5001, Host = "10.5.205.235", Port = 3389 });
            //Tunnels.Add(new Tunnel() { Type = TunnelType.DYNAMIC, ListenIP = "0.0.0.0", ListenPort = 8080 });
        }

        private string getArguments() {
            string command = String.Empty;
            StringBuilder sb = new StringBuilder("-v -ssh");

            if (Username != String.Empty)
                sb.AppendFormat(" -l {0}", Username); //todo quote

            sb.AppendFormat(" -P {0}", Port); //todo quote

            if (Compression)
                sb.Append(" -C");

            sb.Append(X11Forwarding ? " -X" : " -x");
            sb.Append(AgentForwarding ? " -A" : " -a");

            if (!StartShell && !SendCommands)
                sb.Append(" -N");

            if (PrivateKeyFile != String.Empty)
                sb.AppendFormat(" -i {0}", PrivateKeyFile); //todo quote

            sb.Append(UsePageant ? " -agent" : " -noagent");

            if (SendCommands) {
                if (RemoteCommand != String.Empty)
                    command = RemoteCommand;
                else if (RemoteCommandFile != String.Empty)
                    sb.AppendFormat(" -m {0}", RemoteCommandFile); //todo quote

                if (RemoteCommandSubsystem)
                    sb.Append(" -s");
            }
            else
                foreach (Tunnel tunnel in Tunnels)
                    sb.Append(tunnel.ToString(" -{0} {1}:{2}", ":{3}:{4}")); //todo quote

            sb.AppendFormat(" {0}", Host); //todo quote

            if (command != String.Empty)
                sb.AppendFormat(" {0}", command); //todo quote

            return sb.ToString();
        }

        public void SetViewModel(ViewModel viewModel) {
            this.viewModel = viewModel;
        }


        public ViewModel GetViewModel() {
            return viewModel;
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

            processExit = false;

            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = App.Plink,
                Arguments = getArguments(),
                UseShellExecute = false,
                //CreateNoWindow = true,
                RedirectStandardError = true,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            process = Process.Start(startInfo);
            if (process == null)
                return;

            process.EnableRaisingEvents = true;
            process.Exited += process_Exited;

            tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => monitorStream(process.StandardError, StandardStreamType.ERROR, tokenSource.Token), tokenSource.Token);
            Task.Factory.StartNew(() => monitorStream(process.StandardOutput, StandardStreamType.OUTPUT, tokenSource.Token), tokenSource.Token);

            Status = SessionStatus.CONNECTED;
        }

        public void Disconnect() {
            if (Status != SessionStatus.CONNECTED)
                return;

            Debug.WriteLine("disconnect: \"" + Name + "\"");

            processExit = true;
            process.Kill();
            process.WaitForExit();
            process.Close();

            Status = SessionStatus.DISCONNECTED;
        }

        private void process_Exited(object sender, EventArgs e) {
            if (!processExit) {
                Debug.WriteLine("exit: \"" + Name + "\"; " + process.ExitCode);

                if (process.ExitCode == 0)
                    Status = SessionStatus.DISCONNECTED;
                else {
                    Status = SessionStatus.ERROR;

                    if (AutoReconnect)
                        (reconnectTimer ?? (
                                reconnectTimer = new Timer(state => Connect())
                            )).Change(new Random().Next(500, 5000), Timeout.Infinite);
                }

                process.Close();
            }
        }

        private async void monitorStream(StreamReader streamReader, StandardStreamType type, CancellationToken ct) {
            char[] buffer = new char[1024];

            while (!streamReader.EndOfStream) {
                if (ct.IsCancellationRequested)
                    return;

                int read = await streamReader.ReadAsync(buffer, 0, 1024);
                if (read > 0)
                    if (StandardDataReceived != null) {
                        if (type == StandardStreamType.ERROR)
                            Debug.Write("");
                        StandardDataReceived(streamReader, new StandardDataReceivedEventArgs(type, new string(buffer, 0, read)));
                    }
            }

            streamReader.Close();
        }

        private void session_StandardDataReceived(object sender, StandardDataReceivedEventArgs e) {
            Debug.Write(e.Type + "---" + e.Data);
            /*if (e.Type == StandardStreamType.ERROR) {
                if (e.Data.StartsWith("*************")) tokenSource.Cancel();
            }*/

            // todo split console functionality off into own class

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
