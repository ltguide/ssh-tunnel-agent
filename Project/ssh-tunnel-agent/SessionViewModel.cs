using Microsoft.Win32;
using ssh_tunnel_agent.Classes;
using ssh_tunnel_agent.Data;
using System;
using System.IO;
using System.Windows.Controls;

namespace ssh_tunnel_agent {
    public class SessionViewModel : NotifyPropertyChangedBase {
        public Session Session { get; private set; }


        public string PopupTitle { get; private set; }

        private Tunnel _tunnel;
        public Tunnel Tunnel {
            get { return _tunnel; }
            set {
                _tunnel = value;
                NotifyPropertyChanged();
            }
        }

        public SessionViewModel(Session session) {
            Session = session;
            if (Session.isEditing)
                PopupTitle = "Edit Session: " + Session.Name;
            else
                PopupTitle = "New Session";

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
                        () => !Session.Tunnels.Contains(Tunnel) && Tunnel.ListenIP != String.Empty && Tunnel.ListenPort != 0 && (Tunnel.Type == TunnelType.DYNAMIC || (Tunnel.Port != 0 && Tunnel.Host != String.Empty))
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
    }
}
