using System;
using System.Globalization;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(int), typeof(int))]
    public class AddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value - 1;
        }
    }
}
