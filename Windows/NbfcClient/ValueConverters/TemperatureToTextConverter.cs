using System;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    public class TemperatureToTextConverter : IMultiValueConverter
    {
        #region Constants

        private const string StringFormat = "Temperature{0}: {1}°C";

        #endregion

        #region IMultiValueConverter implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int temperature = (int)values[0];
            string displayName = values[1] as string;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "";
            }
            else
            {
                displayName = " (" + displayName + ")";
            }

            return string.Format(StringFormat, displayName, temperature);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
