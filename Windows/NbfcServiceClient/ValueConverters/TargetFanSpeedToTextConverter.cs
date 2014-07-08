using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    public class TargetFanSpeedToTextConverter : IMultiValueConverter
    {
        #region Constants

        private const string StringFormat = "{0:0.0}%";
        private const string AutoControledSuffix = " (Auto)";
        private const string CriticalModeSuffix = " (Critical)";

        #endregion

        #region IMultiValueConverter implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = string.Format(StringFormat, (double)values[0]);

            if ((bool)values[2])
            {
                text += CriticalModeSuffix;
            }
            else if ((bool)values[1])
            {
                text += AutoControledSuffix;
            }

            return text;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
