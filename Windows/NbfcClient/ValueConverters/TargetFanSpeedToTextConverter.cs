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
        private const string AutoControlledSuffix = " (Auto)";
        private const string CriticalModeSuffix = " (Critical)";

        #endregion

        #region IMultiValueConverter implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float fanSpeed = (float)values[0];
            bool autoControlEnabled = (bool)values[1];
            bool ciritcalModeEnabled = (bool)values[2];
            string fanSpeedString = string.Format(StringFormat, fanSpeed);

            if (ciritcalModeEnabled)
            {
                fanSpeedString += CriticalModeSuffix;
            }
            else if (autoControlEnabled)
            {
                fanSpeedString += AutoControlledSuffix;
            }

            return fanSpeedString;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
