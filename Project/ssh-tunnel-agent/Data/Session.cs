using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ssh_tunnel_agent.Data {
    public class Session : NotifyPropertyChangedBase {
        public string Name { get; set; }

        private SessionStatus _status;
        public SessionStatus Status {
            get { return _status; }
            set {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public Session(string Name, SessionStatus Status) {
            this.Name = Name;
            this.Status = Status;
        }
    }
}
