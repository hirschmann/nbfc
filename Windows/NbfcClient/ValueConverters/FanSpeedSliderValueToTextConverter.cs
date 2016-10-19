using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    public class FanSpeedSliderValueToTextConverter : IMultiValueConverter
    {
        #region Constants

        private const string StringFormat = "{0:0.0}%";
        private const string AutoControlledText = "Auto";

        #endregion

        #region IMultiValueConverter implementation

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double sliderValue = (double)values[0];
            int fanSpeedSteps = (int)values[1];

            if (fanSpeedSteps == 1)
            {
                return string.Format(StringFormat, 0);
            }
            else
            {
                double speed = (sliderValue / (fanSpeedSteps - 1)) * 100;

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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
        }

        #endregion
    }
}
