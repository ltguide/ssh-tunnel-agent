using ssh_tunnel_agent.Data;
using System.Windows;
using System.Windows.Controls;

namespace ssh_tunnel_agent.Windows {
    /// <summary>
    /// Interaction logic for SessionConfigure.xaml
    /// </summary>
    public partial class SessionConfigure : Window {
        public SessionConfigure(Session session) {
            InitializeComponent();

            DataContext = new SessionViewModel(session);
            dockCommands.DataContext = session.GetViewModel();
        }

        //private void UserControl_Loaded(object sender, RoutedEventArgs e) {
        // make TrayPopup stay open and focus on THIS
        //TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved.StaysOpen = true;
        //WinApi.ActivatePopup(Parent);
        //}

        private void Window_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox == null)
                return;

            if (textBox.Text.Length > 0 && textBox.SelectionStart == 0) textBox.SelectAll();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ((SessionViewModel)border.DataContext).Session.CancelEdit();

            // send focus back to TrayPopup and allow it to close
            //Popup TrayPopupResolved = TaskbarIcon.GetParentTaskbarIcon(this).TrayPopupResolved;
            //TrayPopupResolved.StaysOpen = false;
            //WinApi.ActivatePopup(TrayPopupResolved);
        }
    }
}
