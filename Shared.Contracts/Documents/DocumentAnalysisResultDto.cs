namespace ReminderSaaS.Shared.Contracts.Documents;

public record DocumentAnalysisResultDto(
    string DetectedLanguage,
    string Summary,
    bool ContainsAppointment,
    AppointmentExtractionDto? Appointment
);

public record AppointmentExtractionDto(
    string Title,
    DateOnly Date,
    TimeOnly? Time,
    string? Location,
    string Category
);
