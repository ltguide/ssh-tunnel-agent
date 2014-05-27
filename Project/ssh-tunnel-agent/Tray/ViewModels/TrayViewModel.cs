using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Config;
using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Windows;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Security.Principal;
using System.Windows.Data;
using System.Windows.Input;

namespace ssh_tunnel_agent.Tray {
    public class TrayViewModel : NotifyPropertyChangedBase {
        private bool? _autoStartApplication;
        public bool AutoStartApplication {
            get {
                if (_autoStartApplication == null) {
                    _autoStartApplication = false;

                    try {
                        string taskName = "ssh-tunnel-agent_" + WindowsIdentity.GetCurrent().Name.Replace(@"\", "-");
                        using (TaskService ts = new TaskService()) {
                            Task task = ts.FindTask(taskName);
                            if (task != null) {
                                foreach (Microsoft.Win32.TaskScheduler.Action action in task.Definition.Actions)
                                    if (action.ActionType == TaskActionType.Execute && ((ExecAction)action).Path == '"' + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName) + '"')
                                        _autoStartApplication = true;

                                if (_autoStartApplication == false)
                                    ts.RootFolder.DeleteTask(taskName);
                            }
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
                        () => App.Current.Shutdown()
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
                                string userName = WindowsIdentity.GetCurrent().Name;
                                string taskName = "ssh-tunnel-agent_" + userName.Replace(@"\", "-");
                                using (TaskService ts = new TaskService()) {
                                    if (AutoStartApplication == false) {
                                        bool v2 = ts.HighestSupportedVersion >= new Version(1, 2);
                                        TaskDefinition td = ts.NewTask();

                                        td.RegistrationInfo.Author = "SSH Tunnel Agent";
                                        td.RegistrationInfo.Description = "Start SSH Tunnel Agent on logon";

                                        td.Principal.UserId = userName;
                                        if (v2)
                                            td.Principal.LogonType = TaskLogonType.InteractiveToken;

                                        td.Settings.DisallowStartIfOnBatteries = false;
                                        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                                        LogonTrigger trigger = new LogonTrigger();
                                        if (v2) {
                                            trigger.Delay = TimeSpan.FromSeconds(30);
                                            trigger.UserId = userName;
                                        }
                                        td.Triggers.Add(trigger);

                                        td.Actions.Add(new ExecAction('"' + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName) + '"', null, null));

                                        ts.RootFolder.RegisterTaskDefinition(taskName, td);
                                    }
                                    else
                                        ts.RootFolder.DeleteTask(taskName);
                                }

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
                        (session) => session != null && App.Current.MainWindow == null
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

                            App.Current.MainWindow = new SessionConfigure(session);
                            App.Current.MainWindow.Show();
                        },
                        (session) => {
                            if (App.Current.MainWindow != null)
                                return false;

                            if (session != null && session.IsConnectionOpen)
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

                            App.Current.MainWindow.Close();
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

                            App.Current.MainWindow.Close();
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
                        () => App.Current.MainWindow.Close()
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
                        e.Accepted = session.IsConnectionOpen && !session.SendCommands;
                    };
                }

                return _connectedSessions.View as CollectionView;
            }
            set {
                if (_connectedSessions != null)
                    _connectedSessions.View.Refresh();
            }
        }

        private ObservableCollection<Session> _sessions;
        public ObservableCollection<Session> Sessions {
            get {
                if (_sessions == null) {
                    string value = new Settings(ConfigurationUserLevel.PerUserRoaming).GetCData("Sessions");
                    if (value == "")
                        _sessions = new ObservableCollection<Session>();
                    else {
                        _sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(value);
                        foreach (Session session in _sessions) {
                            session.SetViewModel(this);

                            if (session.AutoConnect)
                                session.Connect();
                        }
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
            string sessions = JsonConvert.SerializeObject(Sessions, Formatting.Indented);

            if (_saveSessions(ConfigurationUserLevel.None, sessions) == null)
                return;

            string message = _saveSessions(ConfigurationUserLevel.PerUserRoaming, sessions);
            if (message != null)
                App.showErrorMessage("Unable to save sessions!" + Environment.NewLine + Environment.NewLine + message);
        }

        private string _saveSessions(ConfigurationUserLevel userLevel, string sessions) {
            Settings settings = new Settings(userLevel);
            settings.SetCData("Sessions", sessions);
            return settings.Save();
        }

        public void DisconnectSessions(bool kill) {
            foreach (Session session in Sessions)
                if (kill)
                    session.Kill();
                else
                    session.Disconnect();
        }
    }
}
