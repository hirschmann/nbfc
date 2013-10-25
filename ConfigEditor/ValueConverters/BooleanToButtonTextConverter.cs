using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ConfigEditor.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToButtonTextConverter : IValueConverter
    {
        #region Constants

        private const string ButtonTextOK = "OK";
        private const string ButtonTextOverwrite = "Overwrite";

        #endregion

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? ButtonTextOK : ButtonTextOverwrite;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
