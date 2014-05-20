using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ssh_tunnel_agent.Windows {
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumComparisonToBoolConverter : MarkupExtension, IValueConverter {
        public EnumComparisonToBoolConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
