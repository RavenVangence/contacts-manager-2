using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ContactsManager.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool used)
            {
                // Green tones for used contacts, neutral for unused
                return used
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E8")) // Light green for used
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")); // Light grey for unused
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
