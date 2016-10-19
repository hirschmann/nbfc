using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = (bool)parameter;
            bool isVisible = (bool)value;

            if (invert)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = (bool)parameter;
            Visibility vis = (Visibility)value;
            bool result = false;

            if (vis == Visibility.Visible)
            {
                result = true;
            }

            if (invert)
            {
                result = !result;
            }

            return result;
        }
    }
}
