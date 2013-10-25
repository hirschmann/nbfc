using System;
using System.Windows.Data;

namespace NbfcServiceClient.ValueConverters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class FanSpeedSliderValueToTextConverter : IMultiValueConverter
    {
        #region Constants

        private const string StringFormat = "{0:0.0}%";
        private const string AutoControlledText = "Auto";
        private const double AutoControlledSpeed = 101;

        #endregion

        #region IMultiValueConverter implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int steps = (int)values[1];

            if (steps == 1)
            {
                return string.Format(StringFormat, 0);
            }
            else
            {
                double speed = ((double)values[0] / (steps - 1)) * 100;

                if (speed >= 0 && speed <= 100)
                {
                    return string.Format(StringFormat, speed);
                }
                else
                {
                    return AutoControlledText;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
