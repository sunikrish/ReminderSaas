using ReminderSaaS.Domain.Entities;

namespace ReminderSaaS.Application.Schedules;

/// <summary>
/// Repository interface for Schedule operations.
/// </summary>
public interface IScheduleRepository
{
    /// <summary>
    /// Gets all schedules for a specific month and year.
    /// </summary>
    Task<List<Schedule>> GetByMonthAsync(int year, int month);

    /// <summary>
    /// Gets a schedule by its ID.
    /// </summary>
    Task<Schedule?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new schedule.
    /// </summary>
    Task<Guid> CreateAsync(Schedule schedule);

    /// <summary>
    /// Updates an existing schedule.
    /// </summary>
    Task UpdateAsync(Schedule schedule);

    /// <summary>
    /// Deletes a schedule by its ID.
    /// </summary>
    Task DeleteAsync(Guid id);
}
