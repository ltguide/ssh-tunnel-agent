using Hardcodet.Wpf.TaskbarNotification;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ssh_tunnel_agent.Tray {
    /// <summary>
    /// Interaction logic for TrayPopupSession.xaml
    /// </summary>
    public partial class TrayPopupSession : UserControl {
        public Session Session { private set; get; }
        public string PopupTitle { private set; get; }

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
            TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved.StaysOpen = true;
            WinApi.ActivatePopup(Parent);
        }

        private void imgClose_MouseUp(object sender, MouseButtonEventArgs e) {
            TaskbarIcon.GetParentTaskbarIcon(this).CloseBalloon();
        }

        private void OnBalloonClosing(object sender, RoutedEventArgs e) {
            Popup trayPopup = TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved;
            trayPopup.StaysOpen = false;
            WinApi.ActivatePopup(trayPopup);
            Session.CancelEdit();
        }
    }
}
