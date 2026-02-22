namespace ReminderSaaS.Domain.Entities;

/// <summary>
/// Schedule entity representing a user's scheduled event or appointment.
/// </summary>
public class Schedule
{
    /// <summary>
    /// Unique identifier for the schedule.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Title of the schedule (required).
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the schedule.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Date of the schedule (required).
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Optional time of the schedule.
    /// </summary>
    public TimeOnly? Time { get; private set; }

    /// <summary>
    /// Optional location of the schedule.
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Category of the schedule.
    /// </summary>
    public ScheduleCategory Category { get; private set; }

    /// <summary>
    /// Timestamp when the schedule was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the schedule was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Schedule() { }

    /// <summary>
    /// Creates a new Schedule.
    /// </summary>
    public Schedule(
        string title,
        DateOnly date,
        ScheduleCategory category,
        string? description = null,
        TimeOnly? time = null,
        string? location = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Date = date;
        Category = category;
        Description = description;
        Time = time;
        Location = location;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the schedule details.
    /// </summary>
    public void Update(
        string title,
        DateOnly date,
        ScheduleCategory category,
        string? description = null,
        TimeOnly? time = null,
        string? location = null)
    {
        Title = title;
        Date = date;
        Category = category;
        Description = description;
        Time = time;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Category enum for schedules.
/// </summary>
public enum ScheduleCategory
{
    Government,
    Tax,
    KidsSchool,
    School,
    Personal,
    Health,
    Car,
    Finance,
    Other
}
