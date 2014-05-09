using ssh_tunnel_agent.Data;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ssh_tunnel_agent {
    public class NotifyIconViewModel : NotifyPropertyChangedBase {
        private RelayCommand _exitApplicationCommand = new RelayCommand((param) => Application.Current.Shutdown());
        public ICommand ExitApplicationCommand {
            get { return _exitApplicationCommand; }
        }

        private RelayCommand _autoStartApplicationCommand;
        public ICommand AutoStartApplicationCommand {
            get {
                if (_autoStartApplicationCommand == null)
                    _autoStartApplicationCommand = new RelayCommand((param) => {
                        AutoStartApplication = !AutoStartApplication;

                        if (AutoStartApplication) {
                            Debug.WriteLine("add to HKCU Run");
                        }
                        else {
                            Debug.WriteLine("remove from HKCU Run");
                        }
                    });

                return _autoStartApplicationCommand;
            }
        }

        private bool _autoStartApplication = Settings.Get<bool>("AutoStartApplication");
        public bool AutoStartApplication {
            set {
                _autoStartApplication = value;
                Settings.Set("AutoStartApplication", value);
                Settings.Save();
                NotifyPropertyChanged();
            }
            get {
                return _autoStartApplication;
            }
        }

        private RelayCommand _triggerSessionCommand;
        public ICommand TriggerSessionCommand {
            get {
                if (_triggerSessionCommand == null)
                    _triggerSessionCommand = new RelayCommand((param) => triggerSession(param));

                return _triggerSessionCommand;
            }
        }

        private void triggerSession(object param) {
            ((Session)param).ToggleConnection();
            ConnectedSessions = null;
        }

        private RelayCommand _configureSessionCommand;
        public ICommand ConfigureSessionCommand {
            get {
                if (_configureSessionCommand == null)
                    _configureSessionCommand = new RelayCommand((param) => configureSession(param));

                return _configureSessionCommand;
            }
        }

        private void configureSession(object param) {
            Session session = (Session)param;

            System.Diagnostics.Debug.WriteLine("configure " + session.Name);
        }

        private string _connectedSessions = "No sessions connected.";
        public string ConnectedSessions {
            set {
                if (value == null) {
                    StringBuilder sb = new StringBuilder();

                    foreach (Session session in Sessions)
                        if (session.Status == SessionStatus.CONNECTED && !session.SendCommands)
                            sb.AppendLine(session.TunnelsToString());

                    _connectedSessions = sb.Length > 0 ? sb.ToString(0, sb.Length - Environment.NewLine.Length) : "No sessions connected.";
                }
                else
                    _connectedSessions = value;

                NotifyPropertyChanged();
            }
            get { return _connectedSessions; }

        }

        private ObservableCollection<Session> _sessions;
        public ObservableCollection<Session> Sessions {
            set {
                _sessions = value;
                NotifyPropertyChanged();
            }
            get {
                if (_sessions == null)
                    _sessions = new ObservableCollection<Session>();

                return _sessions;
            }

        }





        private RelayCommand _testCommand;
        public ICommand TestCommand {
            get {
                if (_testCommand == null) _testCommand = new RelayCommand((param) => {
                    string x = String.Empty;
                    if (Sessions.Count > 0) {
                        x = Sessions[Sessions.Count - 1].Name;
                        TriggerSessionCommand.Execute(Sessions[0]);
                    }
                    _sessions.Add(new Session() {
                        Name = x + Sessions.Count.ToString(),
                        Status = SessionStatus.ERROR
                    });
                });

                return _testCommand;
            }
        }
    }
}
