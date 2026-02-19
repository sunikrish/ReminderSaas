using ReminderSaaS.Shared.Contracts;
using ReminderSaaS.Domain.Entities;

namespace ReminderSaaS.Application.Reminders;

public class ReminderService : IReminderService
{
    private readonly IReminderRepository _repository;

    public ReminderService(IReminderRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateReminderResponse> CreateAsync(CreateReminderRequest request)
    {
        var reminder = new Reminder(
            tenantId: Guid.NewGuid(), // temp until JWT added
            title: request.Title,
            description: request.Description,
            reminderDate: request.ReminderDate
        );

        await _repository.AddAsync(reminder);

        return new CreateReminderResponse(
            reminder.Id,
            reminder.Title,
            reminder.ReminderDate
        );
    }
}
