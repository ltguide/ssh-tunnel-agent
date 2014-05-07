using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ssh_tunnel_agent {
    [ValueConversion(typeof(TunnelStatus), typeof(string))]
    public class TunnelStateToImageSourceConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return "/Assets/TunnelState_" + value.ToString() + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
