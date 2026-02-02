using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace DidIDoThatApp.Converters
{
    /// <summary>
    /// Converts a boolean to its inverse. Used in XAML when you want the opposite visibility.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return false;
        }
    }
}
