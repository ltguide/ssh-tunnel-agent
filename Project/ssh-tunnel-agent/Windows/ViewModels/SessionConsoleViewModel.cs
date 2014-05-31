using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Windows.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ssh_tunnel_agent.Windows {
    public class SessionConsoleViewModel : NotifyPropertyChangedBase {
        private event EventHandler<StandardDataReceivedEventArgs> StandardDataReceived;
        private string[] _splitby = new string[] { "\r\n" };
        private bool _passwordRequested;
        private Session _session;

        public event EventHandler<ConsoleStatusChangedEventArgs> StatusChanged;
        public event EventHandler PasswordRequested;

        public Process Process { get; private set; }
        public string Title { get; private set; }

        private string _standardOutput;
        public string StandardOutput {
            get { return _standardOutput; }
            set {
                _standardOutput = value;
                NotifyPropertyChanged();
            }
        }

        private string _standardError;
        public string StandardError {
            get { return _standardError; }
            set {
                _standardError = value;
                NotifyPropertyChanged();
            }
        }

        private string _standardInput;
        public string StandardInput {
            get { return _standardInput; }
            set {
                if (Status == ConsoleStatus.STOREHOST || Status == ConsoleStatus.UPDATEHOST)
                    StandardError += value + Environment.NewLine;
                else {
                    if (_passwordRequested) {
                        if (value != String.Empty && value != "\x1b\x3") StandardOutput += "***";
                        OnPasswordRequested(); // flip input boxes
                    }
                    else if (Status != ConsoleStatus.ACCESSGRANTED) // server will echo now
                        StandardOutput += value;

                    if (Status == ConsoleStatus.LOGINAS)
                        StandardOutput += Environment.NewLine; // whyyyyy
                }

                Process.StandardInput.Write(value + "\n");
                _standardInput = String.Empty;
                NotifyPropertyChanged();
            }
        }

        private bool _standardInputEnabled = true;
        public bool StandardInputEnabled {
            get { return _standardInputEnabled; }
            set {
                if (value == _standardInputEnabled)
                    return;

                _standardInputEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private ConsoleStatus _status;
        public ConsoleStatus Status {
            get { return _status; }
            set {
                if (_status == value)
                    return;

                _status = value;
                NotifyPropertyChanged();

                if (StatusChanged != null)
                    StatusChanged(this, new ConsoleStatusChangedEventArgs(_status));
            }
        }

        public SessionConsoleViewModel(Process process, Session session) {
            _session = session;
            Process = process;
            Title = "Session Console: " + session.Name;

            StandardDataReceived += session_StandardDataReceived;

            Task.Factory.StartNew(() => monitorStream(Process.StandardError, StandardStreamType.ERROR));
            Task.Factory.StartNew(() => monitorStream(Process.StandardOutput, StandardStreamType.OUTPUT));
        }

        private async void monitorStream(StreamReader streamReader, StandardStreamType type) {
            try {
                char[] buffer = new char[1024];

                using (streamReader)
                    while (!streamReader.EndOfStream) {
                        int read = await streamReader.ReadAsync(buffer, 0, 1024);
                        if (read > 0)
                            OnStandardDataReceived(streamReader, new StandardDataReceivedEventArgs(type, new string(buffer, 0, read)));
                    }
            }
            catch (ObjectDisposedException) { } // in case the stream gets closed elsewhere
            catch (NullReferenceException) { }

            StandardInputEnabled = false;

            Debug.WriteLine("end standard " + type + " stream monitor");
        }

        private void OnStandardDataReceived(object sender, StandardDataReceivedEventArgs e) {
            if (StandardDataReceived != null)
                StandardDataReceived(sender, e);
        }

        public void UnsubscribeStandardDataReceived() {
            StandardDataReceived -= session_StandardDataReceived;
        }

        private void session_StandardDataReceived(object sender, StandardDataReceivedEventArgs e) {
            if (e.Type == StandardStreamType.OUTPUT) {
                if (Status != ConsoleStatus.ACCESSGRANTED) {
                    if (e.Data == "login as: ")
                        Status = ConsoleStatus.LOGINAS;
                    else if (e.Data.EndsWith("'s password: ")) {
                        Status = ConsoleStatus.PASSWORD;
                        OnPasswordRequested();
                    }
                    else if (e.Data.StartsWith("Passphrase for key ")) {
                        Status = ConsoleStatus.PRIVATEKEY;
                        OnPasswordRequested();
                    }

                    StandardOutput += e.Data;
                }
                else {
                    char[] data = new char[e.Data.Length];
                    int read = 0;

                    bool gotESC = false;
                    bool gotOSC = false;
                    bool gotCSI = false;

                    for (var i = 0; i < e.Data.Length; i++) {
                        char c = e.Data[i];

                        if (gotOSC) {
                            if (c == 7) // BEL
                                gotOSC = false;
                        }
                        else if (gotCSI) {
                            if (c >= 64 && c <= 126) // @ to ~
                                gotCSI = false;
                        }
                        else if (gotESC) {
                            if (c == 91) // [
                                gotCSI = true;
                            else if (c == 93) // ]
                                gotOSC = true;

                            gotESC = false;
                        }
                        else if (c == 27) // ESC
                            gotESC = true;
                        else
                            data[read++] = c;
                    }

                    string output = new string(data, 0, read);
                    if (output.EndsWith("Password: "))
                        OnPasswordRequested();
                    else if (output.StartsWith("[sudo] password for "))
                        OnPasswordRequested();

                    StandardOutput += output;
                }
            }

            else if (e.Type == StandardStreamType.ERROR) {
                if (Status != ConsoleStatus.ACCESSGRANTED) {
                    string[] lines = e.Data.Split(_splitby, StringSplitOptions.RemoveEmptyEntries);

                    if (Array.IndexOf(lines, "Access granted") >= 0) {
                        Status = ConsoleStatus.ACCESSGRANTED;
                        _session.Connected();
                    }
                    else if (Array.IndexOf(lines, "Store key in cache? (y/n) ") >= 0)
                        Status = ConsoleStatus.STOREHOST;
                    else if (Array.IndexOf(lines, "Update cached key? (y/n, Return cancels connection) ") >= 0)
                        Status = ConsoleStatus.UPDATEHOST;
                    else if (Array.Find<string>(lines, line => line.StartsWith("FATAL ERROR:")) != null)
                        Status = ConsoleStatus.ERROR;
                }

                StandardError += e.Data;
            }
        }

        private void OnPasswordRequested() {
            _passwordRequested = !_passwordRequested;

            if (PasswordRequested != null)
                PasswordRequested(this, new EventArgs());
        }

        public bool shouldSendCancel() {
            if (Status == ConsoleStatus.ACCESSGRANTED)
                return true;

            _session.Disconnect();
            return false;
        }
    }
}
