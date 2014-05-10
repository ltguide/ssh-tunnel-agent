using Hardcodet.Wpf.TaskbarNotification;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ssh_tunnel_agent.Tray {
    /// <summary>
    /// Interaction logic for TrayPopupSession.xaml
    /// </summary>
    public partial class TrayPopupSession : UserControl {
        private ViewModel viewModel;
        public Session Session { private set; get; }
        private bool newSession = false;
        private bool shouldClose = false;

        public string PopupTitle { set; get; }

        public TrayPopupSession(ViewModel viewModel, Session _session) {
            this.viewModel = viewModel;

            if (_session == null) {
                Session = new Session(viewModel);
                newSession = true;
                PopupTitle = "New Session";
            }
            else {
                Session = _session;
                Session.BeginEdit();
                PopupTitle = "Edit Session: " + Session.Name;
            }

            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        public RelayCommand OkCommand {
            get {
                return new RelayCommand((param) => {
                    if (newSession)
                        viewModel.Sessions.Add(Session);
                    else
                        Session.EndEdit();

                    viewModel.SaveSessions();

                    shouldClose = true;
                    viewModel.TrayIcon.CloseBalloon();
                });
            }
        }

        public RelayCommand CancelCommand {
            get {
                return new RelayCommand((param) => {
                    if (!newSession)
                        Session.CancelEdit();

                    shouldClose = true;
                    newSession = false;
                    viewModel.TrayIcon.CloseBalloon();
                });
            }
        }

        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e) {
            CancelCommand.Execute(null);
        }

        private void OnBalloonClosing(object sender, RoutedEventArgs e) {
            if (shouldClose) {
                viewModel.TrayIcon.TrayPopupResolved.StaysOpen = false;
                WinApi.ActivatePopup(viewModel.TrayIcon.TrayPopupResolved);
            }
            else if (!newSession)
                Session.CancelEdit();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            viewModel.TrayIcon.TrayPopupResolved.StaysOpen = true;
            WinApi.ActivatePopup(Parent);
        }
    }
}
