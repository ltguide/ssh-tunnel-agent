using ssh_tunnel_agent.Data;
using ssh_tunnel_agent.Windows.Classes;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ssh_tunnel_agent.Windows {
    /// <summary>
    /// Interaction logic for SessionConsole.xaml
    /// </summary>
    public partial class SessionConsole : Window {
        private bool stayOpen;

        public SessionConsole(Process process, Session session) {
            InitializeComponent();

            stayOpen = session.StartShell;

            SessionConsoleViewModel viewModel = new SessionConsoleViewModel(process, session.Name);
            viewModel.ConsoleStatusUpdated += DataContext_ConsoleStatusUpdated;

            DataContext = viewModel;

            if (App.Current.MainWindow is SessionConsole)
                App.Current.MainWindow = null;
        }

        void DataContext_ConsoleStatusUpdated(object sender, ConsoleStatus status) {
            Dispatcher.BeginInvoke(new Action(
                    () => {
                        if (status == ConsoleStatus.ACCESSGRANTED && !stayOpen)
                            Close();
                        else
                            Show();
                    }
                ));
        }

        private void txtStandardInput_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;

            txtStandardInput.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void StandardStream_TargetUpdated(object sender, DataTransferEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
                return;

            textBox.ScrollToEnd();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            txtStandardInput.Focus();
        }
    }
}
