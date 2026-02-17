namespace ReminderSaaS.Domain.Entities;

public class Reminder
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public DateTime ReminderDate { get; private set; }

    public bool IsCompleted { get; private set; }

    // Required by EF Core
    private Reminder() { }

    public Reminder(
        Guid tenantId,
        string title,
        string description,
        DateTime reminderDate)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Title = title;
        Description = description;
        ReminderDate = reminderDate;
        IsCompleted = false;
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }

    public void UpdateDetails(string title, string description, DateTime reminderDate)
    {
        Title = title;
        Description = description;
        ReminderDate = reminderDate;
    }
}

