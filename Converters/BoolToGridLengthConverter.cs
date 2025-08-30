using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ContactsManager.Converters
{
    public class BoolToGridLengthConverter : IValueConverter
    {
        private static readonly GridLengthConverter glc = new();

        // ConverterParameter format: "TrueValue;FalseValue"
        // Each value can be like: "*", "2*", "Auto", "16" etc.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string trueVal = "*";
            string falseVal = "0";

            if (parameter is string param && param.Contains(';'))
            {
                var parts = param.Split(';');
                if (parts.Length >= 2)
                {
                    trueVal = parts[0];
                    falseVal = parts[1];
                }
            }

            var pick = value is bool b && b ? trueVal : falseVal;
            try
            {
                return (GridLength?)glc.ConvertFromString(pick) ?? new GridLength(0);
            }
            catch
            {
                return new GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gl)
            {
                // Heuristic: if width is zero treat as false; otherwise true
                return gl.Value > 0;
            }
            return false;
        }
    }
}
