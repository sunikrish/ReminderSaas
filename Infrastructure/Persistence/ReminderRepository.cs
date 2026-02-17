using ReminderSaaS.Application.Reminders;
using ReminderSaaS.Domain.Entities;

namespace ReminderSaaS.Infrastructure.Persistence;

public class ReminderRepository : IReminderRepository
{
    private readonly AppDbContext _context;

    public ReminderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reminder reminder)
    {
        await _context.Reminders.AddAsync(reminder);
        await _context.SaveChangesAsync();
    }
}
