using System;
using System.Globalization;
using System.Windows.Data;

namespace ScreenCapture
{
    [ValueConversion(typeof(bool), typeof(string))]
    class PinUnpinnedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
                return isPinned ? "\xE840" : "\xE718";

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
