using System.Globalization;

namespace ReminderSaaS.Maui.Converters;

/// <summary>
/// Converts between DateOnly (ViewModel) and DateTime (DatePicker control).
/// </summary>
public class DateOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }
        return DateTime.Today;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }
        return DateOnly.FromDateTime(DateTime.Today);
    }
}
