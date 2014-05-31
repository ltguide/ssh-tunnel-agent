using ssh_tunnel_agent.Data;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ssh_tunnel_agent.Tray {
    [ValueConversion(typeof(SessionStatus), typeof(string))]
    public class SessionStatusToCommandTextConverter : MarkupExtension, IValueConverter {
        public SessionStatusToCommandTextConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((SessionStatus)value) {
                case SessionStatus.CONNECTING:
                case SessionStatus.CONNECTED:
                    return "Disconnect";
                case SessionStatus.ERROR:
                    return "Reconnect";
                case SessionStatus.DISCONNECTED:
                default:
                    return "Connect";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
