using System.Diagnostics;
using System.Text.Json;

namespace ReminderSaaS.Maui.Services;

public class DocumentScanService : IDocumentScanService
{
    private readonly HttpClient _httpClient;
    private const string ScanEndpoint = "http://localhost:7071/api/scan-document";
    private const string ScanEndpointAndroid = "http://10.0.2.2:7071/api/scan-document";

    public DocumentScanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsCameraAvailableAsync()
    {
        try
        {
            var status = await MediaPicker.CapturePhotoAsync();
            return status != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Camera availability check error: {ex.Message}");
            return false;
        }
    }

    public async Task<DocumentScanResult?> CaptureAndAnalyzeAsync()
    {
        try
        {
            // Request camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                Debug.WriteLine("Camera permission denied");
                return null;
            }

            // Capture photo
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null)
                return null;

            // Read image into memory
            byte[] imageData;
            using (var stream = await photo.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            // Send to backend
            var endpoint = DeviceInfo.Platform == DevicePlatform.Android 
                ? ScanEndpointAndroid 
                : ScanEndpoint;

            var content = new ByteArrayContent(imageData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            Debug.WriteLine($"Sending image to {endpoint}, size: {imageData.Length} bytes");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Backend error: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Backend response: {json}");

            var result = JsonSerializer.Deserialize<BackendResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
                return null;

            return new DocumentScanResult
            {
                DetectedLanguage = result.DetectedLanguage,
                Summary = result.Summary,
                ContainsAppointment = result.ContainsAppointment,
                Appointment = result.Appointment != null ? new AppointmentData
                {
                    Title = result.Appointment.Title,
                    Date = DateOnly.ParseExact(result.Appointment.Date, "yyyy-MM-dd"),
                    Time = string.IsNullOrEmpty(result.Appointment.Time) 
                        ? null 
                        : TimeOnly.ParseExact(result.Appointment.Time, "HH:mm"),
                    Location = result.Appointment.Location,
                    Category = result.Appointment.Category
                } : null
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Document scan error: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    private class BackendResponse
    {
        public string DetectedLanguage { get; set; }
        public string Summary { get; set; }
        public bool ContainsAppointment { get; set; }
        public BackendAppointment? Appointment { get; set; }
    }

    private class BackendAppointment
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string? Time { get; set; }
        public string? Location { get; set; }
        public string Category { get; set; }
    }
}
