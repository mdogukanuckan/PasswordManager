using System.Globalization;

namespace PasswordManager.Client.Converters;

public class BoolToShowHideConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool isMasked ? (isMasked ? "Göster" : "Gizle") : "Göster";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
