using ReminderSaaS.Shared.Contracts;

namespace ReminderSaaS.Application.Reminders;

public interface IReminderService
{
    Task<CreateReminderResponse> CreateAsync(CreateReminderRequest request);
}
