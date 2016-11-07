using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToContrastingBrushConverter : IValueConverter
    {
        static readonly double RelativeLuminanceWhite = GetRelativeLuminance(Colors.White);
        static readonly double RelativeLuminaceBlack = GetRelativeLuminance(Colors.Black);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (Color)value;

            double luminance = GetRelativeLuminance(c);
            double luminanceDifferenceWhite = Math.Abs(luminance - RelativeLuminanceWhite);
            double luminanceDiffereceBlack = Math.Abs(luminance - RelativeLuminaceBlack);

            return (luminanceDifferenceWhite > luminanceDiffereceBlack)
                ? Brushes.White
                : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region Private Methods

        private static double GetRelativeLuminance(Color c)
        {
            // See https://en.wikipedia.org/wiki/Relative_luminance
            return 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
        }

        #endregion
    }
}
