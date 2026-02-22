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

        // Guard against invalid enum values stored in the database (e.g. 'Tax') which
        // would cause EF Core to throw when mapping to the `ScheduleCategory` enum.
        // We project using the provider value of the Category column and only return
        // rows whose Category string matches a known enum name.
        var validCategoryNames = Enum.GetNames(typeof(ScheduleCategory));

        // Build a SQL IN list of valid category names and execute a raw SQL
        // query that returns only rows whose Category matches a known enum
        // name. We use FromSqlRaw to avoid EF attempting to convert unknown
        // database strings to the `ScheduleCategory` enum during materialization.
        var quoted = string.Join(",", validCategoryNames.Select(n => "'" + n.Replace("'", "''") + "'"));
        var sql = $@"SELECT * FROM [Schedules]
WHERE [Date] >= {{0}} AND [Date] <= {{1}} AND [Category] IN ({quoted})
ORDER BY [Date], [Time]";

        var result = _context.Schedules
            .FromSqlRaw(sql, firstDayOfMonth, lastDayOfMonth)
            .AsNoTracking();

        return await result.ToListAsync();
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
