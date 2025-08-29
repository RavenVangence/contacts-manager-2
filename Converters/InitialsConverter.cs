using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ContactsManager.Converters
{
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = value?.ToString() ?? string.Empty;
            var parts = name
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(p => char.ToUpperInvariant(p[0]));
            var initials = string.Concat(parts);
            return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
