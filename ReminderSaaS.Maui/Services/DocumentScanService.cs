using System.Diagnostics;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ReminderSaaS.Maui.Services;

public class DocumentScanService : IDocumentScanService
{
    private readonly HttpClient _httpClient;
    private const string RelativeScanPath = "api/scan-document";

    public DocumentScanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsCameraAvailableAsync()
    {
        try
        {
            // Rely on the platform API to indicate capture support.
            bool captureSupported = false;
            try
            {
                captureSupported = MediaPicker.Default.IsCaptureSupported;
            }
            catch
            {
                captureSupported = false;
            }

            if (!captureSupported)
                return false;

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status == PermissionStatus.Granted)
                return true;

            // Try requesting permission (only prompts on physical devices)
            status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
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
            // Check whether capture is supported on this device (simulator often doesn't support)
            bool captureSupported = true;
            try
            {
                captureSupported = MediaPicker.Default.IsCaptureSupported;
            }
            catch
            {
                captureSupported = false;
            }

            FileResult? photo = null;

            // If capture is not supported (common on iOS simulator), force the photo picker
            if (DeviceInfo.Platform == DevicePlatform.iOS && !captureSupported)
            {
                try
                {
                    // Inform the user on simulator that camera isn't available and we'll open the photo picker
                    try
                    {
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Simulator Notice",
                                "The iOS simulator doesn't support the camera. Opening the photo picker instead.",
                                "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Alert display failed: {ex.Message}");
                    }

                    photo = await MediaPicker.PickPhotoAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Photo pick (simulator forced) error: {ex.Message}");
                    return null;
                }
            }
            else if (captureSupported)
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
                    // Fall back to picking a photo from library
                    try
                    {
                        photo = await MediaPicker.PickPhotoAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Photo pick fallback error: {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    // Capture photo
                    try
                    {
                        photo = await MediaPicker.CapturePhotoAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Capture photo error: {ex.Message}");
                        // On failure (e.g., simulator), fall back to picking a photo
                        try
                        {
                            photo = await MediaPicker.PickPhotoAsync();
                        }
                        catch (Exception pickEx)
                        {
                            Debug.WriteLine($"Photo pick fallback error: {pickEx.Message}");
                            return null;
                        }
                    }
                }
            }
            else
            {
                // Simulator/device doesn't support capture; ask user to pick a photo
                try
                {
                    photo = await MediaPicker.PickPhotoAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Photo pick (no-capture) error: {ex.Message}");
                    return null;
                }
            }

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

            // Log configured backend base address and send to backend
            Debug.WriteLine($"Configured HttpClient.BaseAddress: {_httpClient.BaseAddress}");
            // Send to backend - use injected HttpClient.BaseAddress if available
            Uri requestUri;
            if (_httpClient.BaseAddress != null)
            {
                requestUri = new Uri(_httpClient.BaseAddress, RelativeScanPath);
            }
            else
            {
                throw new InvalidOperationException("HttpClient.BaseAddress is not configured. Set the HttpClient BaseAddress to your backend URL (e.g. http://<machine-ip>:7147).\n" +
                                                    "For Android emulator use http://10.0.2.2:<port> or configure a reachable host address.");
            }

            var content = new ByteArrayContent(imageData);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            Debug.WriteLine($"Sending image to {requestUri}, size: {imageData.Length} bytes");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Backend error: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Backend response: {json}");

            var result = JsonSerializer.Deserialize<BackendResponse?>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null)
                return null;

            AppointmentData? appointment = null;
            if (result.Appointment != null)
            {
                DateOnly parsedDate;
                TimeOnly parsedTime;
                DateOnly.TryParseExact(result.Appointment.Date, "yyyy-MM-dd", out parsedDate);
                TimeOnly? time = null;
                if (!string.IsNullOrEmpty(result.Appointment.Time) && TimeOnly.TryParseExact(result.Appointment.Time, "HH:mm", out parsedTime))
                    time = parsedTime;

                appointment = new AppointmentData
                {
                    Title = result.Appointment.Title,
                    Date = parsedDate,
                    Time = time,
                    Location = result.Appointment.Location,
                    Category = result.Appointment.Category
                };
            }

            return new DocumentScanResult
            {
                DetectedLanguage = result.DetectedLanguage,
                Summary = result.Summary,
                ContainsAppointment = result.ContainsAppointment,
                Appointment = appointment
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
