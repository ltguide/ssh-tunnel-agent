using ssh_tunnel_agent.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ssh_tunnel_agent {
    public class NotifyIconViewModel : NotifyPropertyChangedBase {
        private RelayCommand _exitApplicationCommand = new RelayCommand((param) => Application.Current.Shutdown());
        public ICommand ExitApplicationCommand {
            get { return _exitApplicationCommand; }
        }

        private RelayCommand _configureApplicationCommand = new RelayCommand((param) => {
            System.Diagnostics.Debug.WriteLine("application configure");
        });
        public ICommand ConfigureApplicationCommand {
            get { return _configureApplicationCommand; }
        }

        private RelayCommand _triggerSessionCommand;
        public ICommand TriggerSessionCommand {
            get {
                if (_triggerSessionCommand == null)
                    _triggerSessionCommand = new RelayCommand((param) => triggerSession(param));

                return _triggerSessionCommand;
            }
        }

        private int tmpConnectedSessions = 0;
        private void triggerSession(object param) {
            Session session = (Session)param;

            System.Diagnostics.Debug.WriteLine("toggle " + session.Name + " which is currently " + session.Status);

            if (session.Status == SessionStatus.CONNECTED) {
                session.Status = SessionStatus.DISCONNECTED;
                tmpConnectedSessions--;
            }
            else {
                session.Status = SessionStatus.CONNECTED;
                tmpConnectedSessions++;
            }

            ConnectedSessions = tmpConnectedSessions + " connected tunnels\nnew line";
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
            get { return _connectedSessions; }
            set {
                _connectedSessions = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Session> _sessions;
        public ObservableCollection<Session> Sessions {
            get {
                if (_sessions == null)
                    _sessions = new ObservableCollection<Session>();

                return _sessions;
            }
            set {
                _sessions = value;
                NotifyPropertyChanged();
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
                    _sessions.Add(new Session(x + Sessions.Count.ToString(), SessionStatus.ERROR));
                });
                return _testCommand;
            }
        }
    }
}
