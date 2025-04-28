using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CollectionManager.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditing && parameter is string paramString)
            {
                var strings = paramString.Split('|');
                return isEditing ? strings[0] : strings[1];
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}