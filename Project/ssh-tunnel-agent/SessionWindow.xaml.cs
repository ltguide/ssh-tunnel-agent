using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System.Windows;

namespace ssh_tunnel_agent {
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window {
        private ViewModel viewModel;
        public Session Session { private set; get; }
        private bool newSession = false;
        private bool shouldClose = false;

        public string WindowTitle { set; get; }

        public SessionWindow(ViewModel viewModel, Session _session) {
            this.viewModel = viewModel;

            if (_session == null) {
                Session = new Session(viewModel);
                newSession = true;
                WindowTitle = "New Session";
            }
            else {
                Session = _session;
                Session.BeginEdit();
                WindowTitle = "Edit Session: " + Session.Name;
            }

            InitializeComponent();
        }

        public RelayCommand OkCommand {
            get {
                return new RelayCommand((param) => {
                    if (!newSession)
                        Session.EndEdit();

                    shouldClose = true;
                    Application.Current.MainWindow.Close();
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
                    Application.Current.MainWindow.Close();
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (shouldClose) {
                if (newSession)
                    viewModel.Sessions.Add(Session);
            }
            else if (!newSession)
                Session.CancelEdit();
        }
    }
}
