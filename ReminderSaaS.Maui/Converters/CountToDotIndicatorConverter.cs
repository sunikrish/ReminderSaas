using System.Globalization;

namespace ReminderSaaS.Maui.Converters;

public class CountToDotIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int count)
            return string.Empty;

        if (count == 0)
            return string.Empty;

        // Show up to 3 dots, or "+N" for more than 3
        if (count <= 3)
            return string.Concat(Enumerable.Repeat("●", count));
        else
            return $"●●●";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
