using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        static readonly IReadOnlyDictionary<Color, string> colorNames;

        static ColorToStringConverter()
        {
            var dict = new Dictionary<Color, string>();

            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                var color = (Color)info.GetValue(null);

                if (!dict.ContainsKey(color))
                {
                    dict[color] = info.Name;
                }

                colorNames = dict;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (Color)value;

            if (colorNames.ContainsKey(c))
            {
                return colorNames[c];
            }

            return c.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
