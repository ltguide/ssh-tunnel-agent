using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;

namespace ssh_tunnel_agent.Tray {
    /// <summary>
    /// Interaction logic for TrayToolTip.xaml
    /// </summary>
    public partial class TrayToolTip : UserControl {
        public TrayToolTip() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            ViewModel viewModel = TaskbarIcon.GetParentTaskbarIcon(this).DataContext as ViewModel;
            if (viewModel != null)
                viewModel.ConnectedSessions = null;
        }
    }
}
