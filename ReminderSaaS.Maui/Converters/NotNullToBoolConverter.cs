using System.Globalization;

namespace ReminderSaaS.Maui.Converters;

/// <summary>
/// Converter that returns True if the input is not null, False otherwise.
/// </summary>
public class NotNullToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
