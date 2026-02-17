using ReminderSaaS.Domain.Entities;
using ReminderSaaS.Shared.Contracts.Schedules;

namespace ReminderSaaS.Application.Schedules;

/// <summary>
/// Implementation of schedule business logic service.
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _repository;

    public ScheduleService(IScheduleRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets all schedules for a specific month and year as DTOs.
    /// </summary>
    public async Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month)
    {
        var schedules = await _repository.GetByMonthAsync(year, month);
        return schedules.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets a schedule by its ID as a DTO.
    /// </summary>
    public async Task<ScheduleDto?> GetByIdAsync(Guid id)
    {
        var schedule = await _repository.GetByIdAsync(id);
        return schedule != null ? MapToDto(schedule) : null;
    }

    /// <summary>
    /// Creates a new schedule from DTO.
    /// </summary>
    public async Task<Guid> CreateAsync(CreateScheduleDto dto)
    {
        if (!Enum.TryParse<ScheduleCategory>(dto.Category, true, out var category))
        {
            throw new ArgumentException($"Invalid category: {dto.Category}");
        }

        var schedule = new Schedule(
            dto.Title,
            dto.Date,
            category,
            dto.Description,
            dto.Time,
            dto.Location);

        return await _repository.CreateAsync(schedule);
    }

    /// <summary>
    /// Updates an existing schedule from DTO.
    /// </summary>
    public async Task UpdateAsync(UpdateScheduleDto dto)
    {
        var schedule = await _repository.GetByIdAsync(dto.Id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with ID {dto.Id} not found.");
        }

        if (!Enum.TryParse<ScheduleCategory>(dto.Category, true, out var category))
        {
            throw new ArgumentException($"Invalid category: {dto.Category}");
        }

        schedule.Update(
            dto.Title,
            dto.Date,
            category,
            dto.Description,
            dto.Time,
            dto.Location);

        await _repository.UpdateAsync(schedule);
    }

    /// <summary>
    /// Deletes a schedule by its ID.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// Maps a Schedule entity to ScheduleDto.
    /// </summary>
    private static ScheduleDto MapToDto(Schedule schedule)
    {
        return new ScheduleDto(
            schedule.Id,
            schedule.Title,
            schedule.Description,
            schedule.Date,
            schedule.Time,
            schedule.Location,
            schedule.Category.ToString(),
            schedule.CreatedAt,
            schedule.UpdatedAt);
    }
}
