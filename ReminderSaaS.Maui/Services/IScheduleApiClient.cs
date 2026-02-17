using ReminderSaaS.Shared.Contracts.Schedules;

namespace ReminderSaaS.Maui.Services;

/// <summary>
/// API client interface for schedule operations.
/// </summary>
public interface IScheduleApiClient
{
    /// <summary>
    /// Gets schedules for a specific month and year.
    /// </summary>
    Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month);

    /// <summary>
    /// Gets a schedule by its ID.
    /// </summary>
    Task<ScheduleDto?> GetScheduleByIdAsync(Guid id);

    /// <summary>
    /// Creates a new schedule.
    /// </summary>
    Task<Guid> CreateScheduleAsync(CreateScheduleDto dto);

    /// <summary>
    /// Updates an existing schedule.
    /// </summary>
    Task UpdateScheduleAsync(UpdateScheduleDto dto);

    /// <summary>
    /// Deletes a schedule.
    /// </summary>
    Task DeleteScheduleAsync(Guid id);
}
