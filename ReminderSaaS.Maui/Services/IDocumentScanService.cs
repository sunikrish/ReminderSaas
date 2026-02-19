namespace ReminderSaaS.Maui.Services;

public interface IDocumentScanService
{
    Task<bool> IsCameraAvailableAsync();
    Task<DocumentScanResult?> CaptureAndAnalyzeAsync();
}

public class DocumentScanResult
{
    public string DetectedLanguage { get; set; }
    public string Summary { get; set; }
    public bool ContainsAppointment { get; set; }
    public AppointmentData? Appointment { get; set; }
}

public class AppointmentData
{
    public string Title { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public string? Location { get; set; }
    public string Category { get; set; }
}
