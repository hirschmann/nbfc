using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace NbfcServiceClient.ValueConverters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FanDisplayNameToTextConverter : IMultiValueConverter
    {
        private const string StringFormat = "Fan #{0}";

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = (string)values[0];

            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Format(StringFormat, (int)values[1] + 1);
            }
            else
            {
                return s;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
