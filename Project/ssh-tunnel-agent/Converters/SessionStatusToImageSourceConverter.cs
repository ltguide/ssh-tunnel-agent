using ssh_tunnel_agent.Data;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ssh_tunnel_agent {
    [ValueConversion(typeof(SessionStatus), typeof(string))]
    public class SessionStatusToImageSourceConverter : MarkupExtension, IValueConverter {
        public SessionStatusToImageSourceConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return "/Assets/SessionStatus_" + value.ToString() + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
