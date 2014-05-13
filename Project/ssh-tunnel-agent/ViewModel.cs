using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Config;
using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Tray;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ssh_tunnel_agent {
    public class ViewModel : NotifyPropertyChangedBase {

        private TaskbarIcon _trayIcon;
        public TaskbarIcon TrayIcon {
            get {
                if (_trayIcon == null)
                    _trayIcon = (TaskbarIcon)Application.Current.FindResource("TrayIcon");

                return _trayIcon;
            }
        }

        private bool _autoStartApplication = Settings.Get<bool>("AutoStartApplication");
        public bool AutoStartApplication {
            get {
                return _autoStartApplication;
            }
            set {
                _autoStartApplication = value;
                Settings.Set("AutoStartApplication", value);
                Settings.Save();
                NotifyPropertyChanged();
            }
        }

        private RelayCommand _exitApplicationCommand;
        public RelayCommand ExitApplicationCommand {
            get {
                return _exitApplicationCommand ?? (
                    _exitApplicationCommand = new RelayCommand(
                        () => Application.Current.Shutdown()
                    ));
            }
        }

        private RelayCommand _autoStartApplicationCommand;
        public ICommand AutoStartApplicationCommand {
            get {
                return _autoStartApplicationCommand ?? (
                    _autoStartApplicationCommand = new RelayCommand(
                        () => {
                            AutoStartApplication = !AutoStartApplication;

                            if (AutoStartApplication) {
                                Debug.WriteLine("add to HKCU Run");
                            }
                            else {
                                Debug.WriteLine("remove from HKCU Run");
                            }
                        }
                    ));
            }
        }

        private RelayCommand<Session> _triggerSessionCommand;
        public RelayCommand<Session> TriggerSessionCommand {
            get {
                return _triggerSessionCommand ?? (
                    _triggerSessionCommand = new RelayCommand<Session>(
                        (session) => session.ToggleConnection(),
                        (session) => TrayIcon.CustomBalloon == null
                    ));
            }
        }

        private RelayCommand<Session> _configureSessionCommand;
        public RelayCommand<Session> ConfigureSessionCommand {
            get {
                return _configureSessionCommand ?? (
                    _configureSessionCommand = new RelayCommand<Session>(
                        (session) => {
                            if (session == null)
                                session = new Session(this);
                            else
                                session.BeginEdit();

                            TrayIcon.ShowCustomBalloon(new TrayPopupSession(session), PopupAnimation.Slide, null);
                        },
                        (session) => {
                            if (TrayIcon.CustomBalloon != null)
                                return false;

                            if (session != null && session.Status == SessionStatus.CONNECTED)
                                return false;

                            return true;
                        }
                    ));
            }
        }

        private RelayCommand<Session> _sessionOkCommand;
        public RelayCommand<Session> SessionOkCommand {
            get {
                return _sessionOkCommand ?? (
                    _sessionOkCommand = new RelayCommand<Session>(
                        (session) => {
                            if (session.isEditing)
                                session.EndEdit();
                            else
                                Sessions.Add(session);

                            SaveSessions();

                            TrayIcon.CloseBalloon();
                        },
                        (session) => session != null && session.Name != "" && session.Host != ""
                    ));
            }
        }

        private RelayCommand _sessionCancelCommand;
        public RelayCommand SessionCancelCommand {
            get {
                return _sessionCancelCommand ?? (
                    _sessionCancelCommand = new RelayCommand(
                        () => TrayIcon.CloseBalloon()
                    ));
            }
        }

        private CollectionViewSource _connectedSessions;
        public CollectionView ConnectedSessions {
            get {
                if (_connectedSessions == null) {
                    _connectedSessions = new CollectionViewSource();

                    _connectedSessions.Source = Sessions;
                    _connectedSessions.Filter += (sender, e) => {
                        Session session = e.Item as Session;
                        e.Accepted = session.Status == SessionStatus.CONNECTED && !session.SendCommands;
                    };
                }

                return _connectedSessions.View as CollectionView;
            }
            set { _connectedSessions.View.Refresh(); }
        }

        private ObservableCollection<Session> _sessions;
        public ObservableCollection<Session> Sessions {
            get {
                if (_sessions == null) {
                    string value = Settings.GetCData("Sessions");
                    if (value == "")
                        _sessions = new ObservableCollection<Session>();
                    else {
                        _sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(value);
                        foreach (Session session in _sessions)
                            session.Status = SessionStatus.DISCONNECTED;
                    }
                }

                return _sessions;
            }
            set {
                _sessions = value;
                NotifyPropertyChanged();
            }
        }

        internal void SaveSessions() {
            Settings.SetCData("Sessions", JsonConvert.SerializeObject(Sessions, Formatting.Indented));
            Settings.Save();
        }





        /*private RelayCommand _testCommand;
        public ICommand TestCommand {
            get {
                if (_testCommand == null) _testCommand = new RelayCommand((param) => {
                   
                });

                return _testCommand;
            }
        }*/
    }
}
