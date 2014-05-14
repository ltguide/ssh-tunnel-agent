using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Newtonsoft.Json;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Config;
using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Tray;
using System;
using System.Collections.ObjectModel;
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

        private bool? _autoStartApplication;
        public bool AutoStartApplication {
            get {
                if (_autoStartApplication == null) {
                    _autoStartApplication = false;

                    try {
                        RegistryKey run = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                        object value = run.GetValue("ssh-tunnel-agent");
                        if (value != null) {
                            if (value.ToString() == '"' + Environment.GetCommandLineArgs()[0] + '"')
                                _autoStartApplication = true;
                            else
                                run.DeleteValue("ssh-tunnel-agent");
                        }
                    }
                    catch (Exception) { }
                }

                return (bool)_autoStartApplication;
            }
            set {
                _autoStartApplication = value;
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
                            try {
                                RegistryKey run = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                                if (AutoStartApplication == false) {
                                    run.SetValue("ssh-tunnel-agent", '"' + Environment.GetCommandLineArgs()[0] + '"', RegistryValueKind.String);
                                }
                                else
                                    run.DeleteValue("ssh-tunnel-agent");

                                AutoStartApplication = !AutoStartApplication;
                            }
                            catch (Exception) { }
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

        private RelayCommand<Session> _sessionRemoveCommand;
        public RelayCommand<Session> SessionRemoveCommand {
            get {
                return _sessionRemoveCommand ?? (
                    _sessionRemoveCommand = new RelayCommand<Session>(
                        (session) => {
                            if (session.isEditing)
                                Sessions.Remove(session);

                            SaveSessions();

                            TrayIcon.CloseBalloon();
                        },
                        (session) => session != null && session.isEditing
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
                        (session) => session != null && session.Name != "" && session.Host != "" && ((session.Tunnels.Count > 0 && !session.SendCommands) || ((session.RemoteCommand != String.Empty || session.RemoteCommandFile != String.Empty) && session.SendCommands))
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
                    string value = new Settings().GetCData("Sessions");
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
            Settings settings = new Settings();
            settings.SetCData("Sessions", JsonConvert.SerializeObject(Sessions, Formatting.Indented));
            settings.Save();
        }
    }
}
