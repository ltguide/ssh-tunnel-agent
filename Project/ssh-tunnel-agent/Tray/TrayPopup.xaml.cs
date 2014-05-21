using ssh_tunnel_agent.Data;
using System.Windows;
using System.Windows.Controls;

namespace ssh_tunnel_agent.Tray {
    /// <summary>
    /// Interaction logic for TrayPopup.xaml
    /// </summary>
    public partial class TrayPopup : UserControl {
        public TrayPopup() {
            InitializeComponent();
        }

        private void listSessions_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) {
            resizeLastColumn(sender as ListView);
        }

        private void resizeLastColumn(ListView listView) {
            if (listView == null)
                return;

            GridViewColumnCollection columns = ((GridView)listView.View).Columns;
            if (columns.Count == 0)
                return;

            GridViewColumn column = columns[columns.Count - 1];
            column.Width = column.ActualWidth;
            column.Width = double.NaN;

        }

        private void listSessions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Session item = ((FrameworkElement)e.OriginalSource).DataContext as Session;
            if (item == null)
                return;

            TrayViewModel vm = DataContext as TrayViewModel;
            vm.TriggerSessionCommand.Execute(item);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue == true)
                listSessions.SelectedIndex = -1;
        }
    }
}
