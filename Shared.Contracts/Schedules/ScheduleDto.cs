namespace ReminderSaaS.Shared.Contracts.Schedules;

/// <summary>
/// DTO representing a schedule read from the API.
/// </summary>
public record ScheduleDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly Date,
    TimeOnly? Time,
    string? Location,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
