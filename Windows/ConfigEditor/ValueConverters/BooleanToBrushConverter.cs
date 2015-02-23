using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ConfigEditor.ValueConverters
{
    public class BooleanToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(bool)values[1])
            {
                return Brushes.Tomato;
            }
            else if (!(bool)values[0])
            {
                return Brushes.Gold;
            }
            else
            {
                return SystemColors.ControlLightLightBrush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
