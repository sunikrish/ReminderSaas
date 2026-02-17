using System.Globalization;

namespace ReminderSaaS.Maui.Converters;

/// <summary>
/// Converts a boolean value to a Color based on the provided parameter.
/// Parameter format: "trueColor,falseColor" (e.g., "#FF6B6B,#E0E0E0")
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramStr)
            return Colors.Transparent;

        var colors = paramStr.Split(',');
        if (colors.Length != 2)
            return Colors.Transparent;

        try
        {
            var trueColor = Color.FromArgb(colors[0]);
            var falseColor = Color.FromArgb(colors[1]);
            
            return boolValue ? trueColor : falseColor;
        }
        catch
        {
            return Colors.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
