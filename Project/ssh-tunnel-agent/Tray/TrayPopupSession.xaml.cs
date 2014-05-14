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
        public TrayPopupSession(Session session) {
            InitializeComponent();

            border.DataContext = new SessionViewModel(session);
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            // make TrayPopup stay open and focus on THIS
            TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved.StaysOpen = true;
            WinApi.ActivatePopup(Parent);
        }

        private void OnBalloonClosing(object sender, RoutedEventArgs e) {
            ((SessionViewModel)border.DataContext).Session.CancelEdit();

            // send focus back to TrayPopup and allow it to close
            Popup TrayPopupResolved = TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved;
            TrayPopupResolved.StaysOpen = false;
            WinApi.ActivatePopup(TrayPopupResolved);
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox == null)
                return;

            if (textBox.Text.Length > 0 && textBox.SelectionStart == 0) textBox.SelectAll();
        }
    }
}
