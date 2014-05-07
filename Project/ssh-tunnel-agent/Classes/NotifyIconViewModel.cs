using ssh_tunnel_agent.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ssh_tunnel_agent {
    public class NotifyIconViewModel : NotifyPropertyChangedBase {
        private RelayCommand _exitApplicationCommand = new RelayCommand((param) => Application.Current.Shutdown());
        public ICommand ExitApplicationCommand {
            get { return _exitApplicationCommand; }
        }

        private string _connectedTunnels = "No tunnels connected.";
        public string ConnectedTunnels {
            get { return _connectedTunnels; }
            set {
                _connectedTunnels = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Tunnel> _tunnels;
        public ObservableCollection<Tunnel> Tunnels {
            get {
                if (_tunnels == null) {
                    _tunnels = new ObservableCollection<Tunnel>();
                    _tunnels.Add(new Tunnel("x", TunnelStatus.CONNECTED));
                }
                return _tunnels;
            }
            set {
                _tunnels = value;
                NotifyPropertyChanged();
            }
        }





        private int phoneNumberValue = 0;
        public int PhoneNumber {
            get { return phoneNumberValue; }
            set {
                if (value != phoneNumberValue) {
                    phoneNumberValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private RelayCommand _testCommand;
        public ICommand TestCommand {
            get {
                if (_testCommand == null) _testCommand = new RelayCommand((param) => changePhoneNumber());
                return _testCommand;
            }
        }

        private void changePhoneNumber() {
            PhoneNumber++;
            ConnectedTunnels = PhoneNumber + " connected tunnels\nwoot";
            _tunnels[0].Status = _tunnels[0].Status == TunnelStatus.DISCONNECTED ? TunnelStatus.CONNECTED : TunnelStatus.DISCONNECTED;
            _tunnels.Add(new Tunnel(_tunnels[_tunnels.Count - 1].Session + PhoneNumber.ToString(), TunnelStatus.ERROR));
        }
    }
}
