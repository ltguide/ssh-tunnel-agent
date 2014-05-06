using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ssh_tunnel_agent {
    public class NotifyIconViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private RelayCommand _exitApplicationCommand = new RelayCommand((param) => Application.Current.Shutdown());
        public ICommand ExitApplicationCommand {
            get { return _exitApplicationCommand; }
        }

        private String _connectedTunnels = "No tunnels connected.";
        public String ConnectedTunnels {
            get { return _connectedTunnels; }
            set {
                _connectedTunnels = value;
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
        }
    }
}
