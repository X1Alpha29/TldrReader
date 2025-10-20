using System.Globalization;
using Microsoft.Maui.Controls;
using TldrMaui.Models;

namespace TldrMaui.Converters;

public class FeedKindEqualsConverter : IValueConverter
{
    // parameter should be the enum name, e.g. "Tech"
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FeedKind current && parameter is string param)
            return string.Equals(current.ToString(), param, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
