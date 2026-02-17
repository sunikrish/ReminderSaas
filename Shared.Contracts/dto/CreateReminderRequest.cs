using System;

namespace ReminderSaaS.Shared.Contracts;

public record CreateReminderRequest(
    string Title,
    string Description,
    DateTime ReminderDate
);
