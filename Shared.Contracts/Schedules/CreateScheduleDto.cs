namespace ReminderSaaS.Shared.Contracts.Schedules;

/// <summary>
/// DTO for creating a new schedule.
/// </summary>
public record CreateScheduleDto(
    string Title,
    DateOnly Date,
    string Category,
    string? Description = null,
    TimeOnly? Time = null,
    string? Location = null);
