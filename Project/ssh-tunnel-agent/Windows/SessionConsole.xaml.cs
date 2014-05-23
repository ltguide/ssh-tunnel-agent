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
        private bool _stayOpen;


        public SessionConsole(Process process, Session session) {
            InitializeComponent();

            if (session.SendCommands)
                _stayOpen = session.PersistentConsole;
            else
                _stayOpen = session.StartShell;

            SessionConsoleViewModel viewModel = new SessionConsoleViewModel(process, session);
            viewModel.StatusChanged += DataContext_StatusChanged;
            viewModel.PasswordRequested += DataContext_PasswordRequested;

            DataContext = viewModel;

            if (App.Current.MainWindow is SessionConsole)
                App.Current.MainWindow = null;
        }

        public void DataContext_StatusChanged(object sender, ConsoleStatusChangedEventArgs e) {
            InvokeIfRequired(() => {
                if (e.Status == ConsoleStatus.ACCESSGRANTED && !_stayOpen)
                    CloseConsole();
                else if (!IsVisible) {
                    Show();

                    txtStandardError.ScrollToEnd();
                    txtStandardOutput.ScrollToEnd();
                    txtInput.Focus();
                }
            });
        }

        private void DataContext_PasswordRequested(object sender, EventArgs e) {
            InvokeIfRequired(() => {
                if (txtInput.IsVisible) {
                    txtInput.Visibility = Visibility.Collapsed;
                    txtPassword.Visibility = Visibility.Visible;
                    txtPassword.Focus();
                }
                else {
                    txtInput.Visibility = Visibility.Visible;
                    txtPassword.Visibility = Visibility.Collapsed;
                    txtInput.Focus();
                }
            });
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e) {
            bool update = false;

            if (e.Key == Key.Escape) {
                if (((SessionConsoleViewModel)DataContext).shouldSendCancel()) {
                    txtInput.Text = "\x1b\x3";

                    update = true;
                }
            }
            else if (e.Key == Key.Enter) {
                update = true;
            }

            if (update) {
                txtInput.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                e.Handled = true;
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                txtPassword.Password = String.Empty;
                txtInput_KeyUp(sender, e);
            }
            else if (e.Key == Key.Enter) {
                txtInput.Text = txtPassword.Password;
                txtPassword.Password = String.Empty;

                txtInput_KeyUp(sender, e);
            }
        }

        private void StandardStream_TargetUpdated(object sender, DataTransferEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
                return;

            textBox.ScrollToEnd();
        }

        private void Window_Closed(object sender, EventArgs e) {
            CloseConsole();
        }

        private void InvokeIfRequired(Action action) {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(action);
            else
                action();
        }

        public void TryClose() {
            InvokeIfRequired(() => CloseConsole());
        }

        private void CloseConsole() {
            SessionConsoleViewModel viewModel = DataContext as SessionConsoleViewModel;
            if (viewModel != null) {
                viewModel.StatusChanged -= DataContext_StatusChanged;
                viewModel.PasswordRequested -= DataContext_PasswordRequested;
                viewModel.UnsubscribeStandardDataReceived();
                DataContext = null;
            }

            if (IsLoaded)
                Close();
        }
    }
}
