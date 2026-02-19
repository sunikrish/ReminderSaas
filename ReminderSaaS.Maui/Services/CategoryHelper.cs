namespace ReminderSaaS.Maui.Services;

public static class CategoryHelper
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

    public static string GetIcon(string category)
    {
        return CategoryIcons.TryGetValue(category, out var icon) ? icon : "📌";
    }

    public static string GetIconWithCategory(string category)
    {
        return $"{GetIcon(category)} {category}";
    }
}
