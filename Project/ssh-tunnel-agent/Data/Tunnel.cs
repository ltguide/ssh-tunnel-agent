using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ssh_tunnel_agent.Data {
    public class Tunnel : NotifyPropertyChangedBase {
        public string Session { get; set; }

        private TunnelStatus _status;
        public TunnelStatus Status {
            get { return _status; }
            set {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public Tunnel(string Session, TunnelStatus Status) {
            this.Session = Session;
            this.Status = Status;
        }
    }
}
