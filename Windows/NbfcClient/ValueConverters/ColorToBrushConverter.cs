using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            return brush.Color;
        }
    }
}
