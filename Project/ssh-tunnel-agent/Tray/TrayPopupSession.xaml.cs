using Hardcodet.Wpf.TaskbarNotification;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ssh_tunnel_agent.Tray {
    /// <summary>
    /// Interaction logic for TrayPopupSession.xaml
    /// </summary>
    public partial class TrayPopupSession : UserControl {
        public Session Session { get; private set; }
        public Tunnel Tunnel { get; private set; }
        public string PopupTitle { get; private set; }

        public TrayPopupSession(Session session) {
            Session = session;
            if (Session.isEditing)
                PopupTitle = "Edit Session: " + Session.Name;
            else
                PopupTitle = "New Session";

            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            // make TrayPopup stay open and focus on THIS
            TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved.StaysOpen = true;
            WinApi.ActivatePopup(Parent);
        }

        private void OnBalloonClosing(object sender, RoutedEventArgs e) {
            Session.CancelEdit();

            // send focus back to TrayPopup and allow it to close
            Popup TrayPopupResolved = TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved;
            TrayPopupResolved.StaysOpen = false;
            WinApi.ActivatePopup(TrayPopupResolved);
        }
    }
}
