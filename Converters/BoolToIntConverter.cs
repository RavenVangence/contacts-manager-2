using System;
using System.Globalization;
using System.Windows.Data;

namespace ContactsManager.Converters
{
    public class BoolToIntConverter : IValueConverter
    {
        // Converts bool to int. By default: true => 1, false => 3
        // Optional ConverterParameter in format "T;F" to specify custom values, e.g. "2;1"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int trueVal = 1;
            int falseVal = 3;

            if (parameter is string param && param.Contains(";"))
            {
                var parts = param.Split(';');
                if (parts.Length >= 2)
                {
                    int.TryParse(parts[0], out trueVal);
                    int.TryParse(parts[1], out falseVal);
                }
            }

            if (value is bool b)
                return b ? trueVal : falseVal;

            return falseVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                // If parameter is provided we try to map back, otherwise default true when 1
                if (parameter is string param && param.Contains(";"))
                {
                    var parts = param.Split(';');
                    if (parts.Length >= 2 && int.TryParse(parts[0], out var t) && int.TryParse(parts[1], out var f))
                    {
                        return i == t;
                    }
                }
                return i == 1;
            }
            return false;
        }
    }
}
