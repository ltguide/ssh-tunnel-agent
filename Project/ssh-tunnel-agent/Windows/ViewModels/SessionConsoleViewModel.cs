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
                    if (Status == ConsoleStatus.PASSWORD)
                        StandardOutput += "***";
                    else if (Status != ConsoleStatus.ACCESSGRANTED)
                        StandardOutput += value;

                    if (Status == ConsoleStatus.LOGINAS)
                        StandardOutput += Environment.NewLine;
                }

                Process.StandardInput.WriteLine(value);
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
                if (read > 0) {
                    char[] data = new char[read];
                    Array.Copy(buffer, data, read);
                    StandardDataReceived(streamReader, new StandardDataReceivedEventArgs(type, data));
                }
            }

            streamReader.Close();
            StandardInputEnabled = false;
        }

        private void session_StandardDataReceived(object sender, StandardDataReceivedEventArgs e) {
            if (e.Type == StandardStreamType.OUTPUT) {
                if (e.Value == "login as: ")
                    Status = ConsoleStatus.LOGINAS;
                else if (e.Value.EndsWith("'s password: "))
                    Status = ConsoleStatus.PASSWORD;

                if (Status != ConsoleStatus.ACCESSGRANTED)
                    StandardOutput += e.Value;
                else {
                    // todo strip ESCAPE codes

                    StandardOutput += e.Value;
                }
            }

            else if (e.Type == StandardStreamType.ERROR) {
                if (Status != ConsoleStatus.ACCESSGRANTED) {
                    string[] lines = e.Value.Split(_splitby, StringSplitOptions.RemoveEmptyEntries);

                    if (Array.IndexOf(lines, "Store key in cache? (y/n) ") >= 0)
                        Status = ConsoleStatus.STOREHOST;
                    else if (Array.IndexOf(lines, "Update cached key? (y/n, Return cancels connection) ") >= 0)
                        Status = ConsoleStatus.UPDATEHOST;
                    else if (Array.IndexOf(lines, "Access granted") >= 0)
                        Status = ConsoleStatus.ACCESSGRANTED;
                }

                StandardError += e.Value;
            }
        }
    }
}
