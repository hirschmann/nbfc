using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    public class FanDisplayNameToTextConverter : IMultiValueConverter
    {
        private const string StringFormat = "Fan #{0}";

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string displayName = (string)values[0];            

            if (string.IsNullOrWhiteSpace(displayName))
            {
                int fanIndex = (int)values[1];
                return string.Format(StringFormat, fanIndex + 1);
            }
            else
            {
                return displayName;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
