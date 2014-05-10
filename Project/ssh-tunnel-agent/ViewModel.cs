using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Config;
using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Tray;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
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
                    _triggerSessionCommand = new RelayCommand((param) => {
                        ((Session)param).ToggleConnection();

                        //ConnectedSessions = null; // this should be done from Session
                    });

                return _triggerSessionCommand;
            }
        }

        private RelayCommand _configureSessionCommand;
        public ICommand ConfigureSessionCommand {
            get {
                if (_configureSessionCommand == null)
                    _configureSessionCommand = new RelayCommand(
                        (param) => {
                            TrayIcon.ShowCustomBalloon(new TrayPopupSession(this, param as Session), PopupAnimation.Slide, null);
                        },
                        (param) => {
                            if (TrayIcon.CustomBalloon != null)
                                return false;

                            Session session = param as Session;
                            if (session != null && session.Status == SessionStatus.CONNECTED)
                                return false;

                            return true;
                        }
                    );

                return _configureSessionCommand;
            }
        }

        public string ConnectedSessions {
            set {
                NotifyPropertyChanged();
            }
            get {
                StringBuilder sb = new StringBuilder();

                foreach (Session session in Sessions)
                    if (session.Status == SessionStatus.CONNECTED && !session.SendCommands)
                        sb.AppendLine(session.TunnelsToString());

                return sb.Length > 0 ? sb.ToString(0, sb.Length - Environment.NewLine.Length) : "No sessions connected.";
            }

        }

        internal void SaveSessions() {
            Settings.SetCData("Sessions", JsonConvert.SerializeObject(Sessions, Formatting.Indented));
            Settings.Save();
        }

        private ObservableCollection<Session> _sessions;
        public ObservableCollection<Session> Sessions {
            set {
                _sessions = value;
                NotifyPropertyChanged();
            }
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
