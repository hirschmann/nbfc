using System;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanToInverseConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
