using System.Globalization;

namespace ReminderSaaS.Maui.Converters;

public class CategoryIconConverter : IValueConverter
{
    private static readonly Dictionary<string, string> CategoryIcons = new()
    {
        { "Government", "📋" },
        { "Kids School", "🎓" },
        { "Personal", "👤" },
        { "Health", "🏥" },
        { "Car", "🚗" },
        { "Finance", "💰" }
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string category && CategoryIcons.TryGetValue(category, out var icon))
        {
            return $"{icon} {category}";
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            // Remove icon and return just the category name
            foreach (var (category, icon) in CategoryIcons)
            {
                if (text.StartsWith(icon))
                {
                    return category;
                }
            }
            return text;
        }
        return value;
    }
}
