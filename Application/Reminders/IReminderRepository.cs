namespace ReminderSaaS.Application.Reminders;
using ReminderSaaS.Domain.Entities;

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder);
}
