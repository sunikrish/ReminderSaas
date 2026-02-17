using Microsoft.EntityFrameworkCore;
using ReminderSaaS.Application.Schedules;
using ReminderSaaS.Domain.Entities;

namespace ReminderSaaS.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Schedule operations.
/// </summary>
public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDbContext _context;

    public ScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all schedules for a specific month and year.
    /// </summary>
    public async Task<List<Schedule>> GetByMonthAsync(int year, int month)
    {
        var firstDayOfMonth = new DateOnly(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        return await _context.Schedules
            .Where(s => s.Date >= firstDayOfMonth && s.Date <= lastDayOfMonth)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Time)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Gets a schedule by its ID.
    /// </summary>
    public async Task<Schedule?> GetByIdAsync(Guid id)
    {
        return await _context.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Creates a new schedule.
    /// </summary>
    public async Task<Guid> CreateAsync(Schedule schedule)
    {
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule.Id;
    }

    /// <summary>
    /// Updates an existing schedule.
    /// </summary>
    public async Task UpdateAsync(Schedule schedule)
    {
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a schedule by its ID.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
