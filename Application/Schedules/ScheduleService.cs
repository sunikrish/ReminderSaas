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
        var category = ParseCategory(dto.Category);

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

        var category = ParseCategory(dto.Category);

        schedule.Update(
            dto.Title,
            dto.Date,
            category,
            dto.Description,
            dto.Time,
            dto.Location);

        await _repository.UpdateAsync(schedule);
    }

    private static ScheduleCategory ParseCategory(string? categoryStr)
    {
        if (string.IsNullOrWhiteSpace(categoryStr))
            return ScheduleCategory.Personal;

        // Remove whitespace and non-alphanumeric chars to match enum identifiers
        var normalized = new string(categoryStr.Where(char.IsLetterOrDigit).ToArray());

        if (Enum.TryParse<ScheduleCategory>(normalized, true, out var category))
            return category;

        throw new ArgumentException($"Invalid category: {categoryStr}");
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
        // Convert enum identifier to frontend display string (e.g. KidsSchool -> "Kids School")
        string displayCategory = schedule.Category switch
        {
            ScheduleCategory.KidsSchool => "Kids School",
            ScheduleCategory.Government => "Government",
            ScheduleCategory.Finance => "Finance",
            ScheduleCategory.Car => "Car",
            ScheduleCategory.School => "Kids School",
            ScheduleCategory.Personal => "Personal",
            ScheduleCategory.Health => "Health",
            _ => schedule.Category.ToString()
        };

        return new ScheduleDto(
            schedule.Id,
            schedule.Title,
            schedule.Description,
            schedule.Date,
            schedule.Time,
            schedule.Location,
            displayCategory,
            schedule.CreatedAt,
            schedule.UpdatedAt);
    }
}
