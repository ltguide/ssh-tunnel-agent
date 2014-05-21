using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Windows.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ssh_tunnel_agent.Windows {
    public class SessionConsoleViewModel : NotifyPropertyChangedBase {
        private event EventHandler<StandardDataReceivedEventArgs> StandardDataReceived;
        public event EventHandler<ConsoleStatus> ConsoleStatusUpdated;
        private CancellationTokenSource tokenSource;
        private string[] _splitby = new string[] { "\r\n" };

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
                    if (Status == ConsoleStatus.PASSWORD || Status == ConsoleStatus.PRIVATEKEY)
                        StandardOutput += "***";
                    else if (Status != ConsoleStatus.ACCESSGRANTED)
                        StandardOutput += value;

                    if (Status == ConsoleStatus.LOGINAS)
                        StandardOutput += Environment.NewLine;
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
                _status = value;
                NotifyPropertyChanged();

                if (ConsoleStatusUpdated != null)
                    ConsoleStatusUpdated(this, _status);
            }
        }

        public SessionConsoleViewModel(Process process, string name) {
            Process = process;
            Title = "Session Console: " + name;

            StandardDataReceived += session_StandardDataReceived;

            tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => monitorStream(Process.StandardError, StandardStreamType.ERROR, tokenSource.Token), tokenSource.Token);
            Task.Factory.StartNew(() => monitorStream(Process.StandardOutput, StandardStreamType.OUTPUT, tokenSource.Token), tokenSource.Token);
        }

        private async void monitorStream(StreamReader streamReader, StandardStreamType type, CancellationToken ct) {
            char[] buffer = new char[1024];

            while (!streamReader.EndOfStream) {
                if (ct.IsCancellationRequested)
                    return;

                int read = await streamReader.ReadAsync(buffer, 0, 1024);
                if (read > 0)
                    StandardDataReceived(streamReader, new StandardDataReceivedEventArgs(type, new string(buffer, 0, read)));
            }

            streamReader.Close();
            StandardInputEnabled = false;
        }

        private void session_StandardDataReceived(object sender, StandardDataReceivedEventArgs e) {
            if (e.Type == StandardStreamType.OUTPUT) {
                if (e.Data == "login as: ")
                    Status = ConsoleStatus.LOGINAS;
                else if (e.Data.EndsWith("'s password: "))
                    Status = ConsoleStatus.PASSWORD;
                else if (e.Data.StartsWith("Passphrase for key "))
                    Status = ConsoleStatus.PRIVATEKEY;

                if (Status != ConsoleStatus.ACCESSGRANTED)
                    StandardOutput += e.Data;
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

                    StandardOutput += new string(data, 0, read);
                }
            }

            else if (e.Type == StandardStreamType.ERROR) {
                if (Status != ConsoleStatus.ACCESSGRANTED) {
                    string[] lines = e.Data.Split(_splitby, StringSplitOptions.RemoveEmptyEntries);

                    if (Array.IndexOf(lines, "Store key in cache? (y/n) ") >= 0)
                        Status = ConsoleStatus.STOREHOST;
                    else if (Array.IndexOf(lines, "Update cached key? (y/n, Return cancels connection) ") >= 0)
                        Status = ConsoleStatus.UPDATEHOST;
                    else if (Array.IndexOf(lines, "Access granted") >= 0)
                        Status = ConsoleStatus.ACCESSGRANTED;
                }

                StandardError += e.Data;
            }
        }
    }
}
