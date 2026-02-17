using System;

namespace ReminderSaaS.Shared.Contracts;

public record CreateReminderResponse(
    Guid Id,
    string Title,
    DateTime ReminderDate
);
