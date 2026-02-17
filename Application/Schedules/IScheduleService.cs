using ReminderSaaS.Shared.Contracts.Schedules;

namespace ReminderSaaS.Application.Schedules;

/// <summary>
/// Service interface for Schedule business logic.
/// </summary>
public interface IScheduleService
{
    /// <summary>
    /// Gets all schedules for a specific month and year as DTOs.
    /// </summary>
    Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month);

    /// <summary>
    /// Gets a schedule by its ID as a DTO.
    /// </summary>
    Task<ScheduleDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new schedule from DTO.
    /// </summary>
    Task<Guid> CreateAsync(CreateScheduleDto dto);

    /// <summary>
    /// Updates an existing schedule from DTO.
    /// </summary>
    Task UpdateAsync(UpdateScheduleDto dto);

    /// <summary>
    /// Deletes a schedule by its ID.
    /// </summary>
    Task DeleteAsync(Guid id);
}
