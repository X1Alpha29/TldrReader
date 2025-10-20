using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using TldrMaui.Models;

namespace TldrMaui.Converters
{
    public class FeedKindToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FeedKind kind)
            {
                return kind switch
                {
                    FeedKind.Tech => Color.FromArgb("#2A4FDB"),
                    FeedKind.AI => Color.FromArgb("#8B5CF6"),
                    FeedKind.Design => Color.FromArgb("#EF4444"),
                    FeedKind.Crypto => Color.FromArgb("#10B981"),
                    _ => Application.Current?.Resources.TryGetValue("TLDR.TextLight", out var c) == true ? c : Colors.Black
                };
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
