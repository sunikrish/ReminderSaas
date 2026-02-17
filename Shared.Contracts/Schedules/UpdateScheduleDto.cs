namespace ReminderSaaS.Shared.Contracts.Schedules;

/// <summary>
/// DTO for updating an existing schedule.
/// </summary>
public record UpdateScheduleDto(
    Guid Id,
    string Title,
    DateOnly Date,
    string Category,
    string? Description = null,
    TimeOnly? Time = null,
    string? Location = null);
