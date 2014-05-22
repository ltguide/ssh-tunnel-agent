using Microsoft.Win32;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace ssh_tunnel_agent.Windows {
    public class SessionConfigureViewModel : NotifyPropertyChangedBase {
        public Session Session { get; private set; }
        public string Title { get; private set; }

        private Tunnel _tunnel;
        public Tunnel Tunnel {
            get { return _tunnel; }
            set {
                _tunnel = value;
                NotifyPropertyChanged();
            }
        }

        public SessionConfigureViewModel(Session session) {
            Session = session;
            if (Session.isEditing)
                Title = "Edit Session: " + Session.Name;
            else
                Title = "New Session";

            Tunnel = new Tunnel();
            //Tunnel = new Tunnel() { Type = TunnelType.REMOTE, ListenIP = "192.168.1.105", ListenPort = 4444, Host = "10.5.205.235", Port = 3389 };
        }

        private RelayCommand _addTunnelCommand;
        public RelayCommand AddTunnelCommand {
            get {
                return _addTunnelCommand ?? (
                    _addTunnelCommand = new RelayCommand(
                        () => {
                            Session.Tunnels.Add(Tunnel);
                            Tunnel = new Tunnel();
                        },
                        () => {
                            if (Session.Tunnels.Contains(Tunnel))
                                return false;

                            if (String.IsNullOrEmpty(Tunnel.ListenIP) || Tunnel["ListenIP"] != null || String.IsNullOrEmpty(Tunnel.ListenPort) || Tunnel["ListenPort"] != null)
                                return false;

                            if (Tunnel.Type != TunnelType.DYNAMIC)
                                if (String.IsNullOrEmpty(Tunnel.Host) || Tunnel["Host"] != null || String.IsNullOrEmpty(Tunnel.Port) || Tunnel["Port"] != null)
                                    return false;

                            return true;
                        }
                    ));
            }
        }

        private RelayCommand<Tunnel> _removeTunnelCommand;
        public RelayCommand<Tunnel> RemoveTunnelCommand {
            get {
                return _removeTunnelCommand ?? (
                    _removeTunnelCommand = new RelayCommand<Tunnel>(
                        (tunnel) => {
                            Tunnel = tunnel;
                            Session.Tunnels.Remove(tunnel);
                        },
                        (tunnel) => tunnel != null
                    ));
            }
        }

        private RelayCommand _clearTunnelCommand;
        public RelayCommand ClearTunnelCommand {
            get {
                return _clearTunnelCommand ?? (
                    _clearTunnelCommand = new RelayCommand(
                        () => Tunnel = new Tunnel()
                    ));
            }
        }

        private RelayCommand<TextBox> _browseCommand;
        public RelayCommand<TextBox> BrowseCommand {
            get {
                return _browseCommand ?? (
                    _browseCommand = new RelayCommand<TextBox>(
                        (textBox) => {
                            OpenFileDialog dlg = new OpenFileDialog();
                            dlg.Filter = textBox.Tag.ToString();

                            if (textBox.Text.Length > 0)
                                dlg.InitialDirectory = Path.GetDirectoryName(textBox.Text);
                            else
                                dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                            if (dlg.ShowDialog() == true) {
                                textBox.Text = dlg.FileName;
                                textBox.Focus();
                                textBox.CaretIndex = textBox.Text.Length;
                                textBox.ScrollToEnd();
                            }
                        }
                    ));
            }
        }

        private string _puttygen;
        private RelayCommand _launchPuTTYgenCommand;
        public RelayCommand LaunchPuTTYgenCommand {
            get {
                return _launchPuTTYgenCommand ?? (
                    _launchPuTTYgenCommand = new RelayCommand(
                        () => {
                            if ((_puttygen ?? (
                                _puttygen = App.FindFile("puttygen.exe", new Version(0, 63))
                            )) == null)
                                App.showErrorMessage("Failed to find up-to-date puttygen.exe.");
                            else {
                                ProcessStartInfo startInfo = new ProcessStartInfo {
                                    FileName = _puttygen,
                                    UseShellExecute = false,
                                };

                                Process.Start(startInfo);
                            }
                        }
                    ));
            }
        }
    }
}
