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

            DataContext = new SessionConfigureViewModel(session);
            dockCommands.DataContext = session.GetViewModel();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox == null)
                return;

            if (textBox.Text.Length > 0 && textBox.SelectionStart == 0) textBox.SelectAll();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ((SessionConfigureViewModel)frame.DataContext).Session.CancelEdit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            txtSessionName.Focus();
        }
    }
}
