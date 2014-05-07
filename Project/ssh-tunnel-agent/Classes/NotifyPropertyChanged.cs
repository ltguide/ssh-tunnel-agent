using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace ssh_tunnel_agent {
    public class NotifyPropertyChangedBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
