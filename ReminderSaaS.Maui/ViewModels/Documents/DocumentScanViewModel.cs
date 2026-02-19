using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReminderSaaS.Maui.Services;
using ReminderSaaS.Shared.Contracts.Schedules;
using System.Diagnostics;

namespace ReminderSaaS.Maui.ViewModels.Documents;

public partial class DocumentScanViewModel : ObservableObject
{
    private readonly IDocumentScanService _documentScanService;
    private readonly IScheduleApiClient _scheduleApiClient;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool showResult;

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private bool containsAppointment;

    [ObservableProperty]
    private string appointmentTitle = string.Empty;

    [ObservableProperty]
    private DateOnly appointmentDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private TimeOnly? appointmentTime;

    [ObservableProperty]
    private string appointmentLocation = string.Empty;

    [ObservableProperty]
    private string appointmentCategory = "Personal";

    [ObservableProperty]
    private string errorMessage = string.Empty;

    private DocumentScanResult? _currentScanResult;

    public DocumentScanViewModel(IDocumentScanService documentScanService, IScheduleApiClient scheduleApiClient)
    {
        _documentScanService = documentScanService;
        _scheduleApiClient = scheduleApiClient;
    }

    [RelayCommand]
    public async Task ScanDocumentAsync()
    {
        try
        {
            ShowResult = false;
            IsLoading = true;
            ErrorMessage = string.Empty;

            Debug.WriteLine("Starting document scan...");
            var result = await _documentScanService.CaptureAndAnalyzeAsync();

            if (result == null)
            {
                ErrorMessage = "Failed to scan document. Please try again.";
                Debug.WriteLine("Scan returned null");
                return;
            }

            _currentScanResult = result;
            Summary = result.Summary;
            ContainsAppointment = result.ContainsAppointment;

            if (result.ContainsAppointment && result.Appointment != null)
            {
                AppointmentTitle = result.Appointment.Title;
                AppointmentDate = result.Appointment.Date;
                AppointmentTime = result.Appointment.Time;
                AppointmentLocation = result.Appointment.Location ?? string.Empty;
                AppointmentCategory = result.Appointment.Category;

                Debug.WriteLine($"Appointment extracted: {result.Appointment.Title} on {result.Appointment.Date}");
            }

            ShowResult = true;
            Debug.WriteLine("Scan completed successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            Debug.WriteLine($"Scan error: {ex.Message}\n{ex.StackTrace}");
            ShowResult = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ConfirmAppointmentAsync()
    {
        try
        {
            IsLoading = true;

            // Create schedule DTO from appointment data
            var createDto = new CreateScheduleDto(
                AppointmentTitle,
                AppointmentDate,
                AppointmentCategory,
                $"Extracted from document",
                AppointmentTime,
                string.IsNullOrEmpty(AppointmentLocation) ? null : AppointmentLocation
            );

            var scheduleId = await _scheduleApiClient.CreateScheduleAsync(createDto);
            
            Debug.WriteLine($"Schedule created with ID: {scheduleId}");

            // Clear data
            ClearScanData();
            ShowResult = false;

            // Show success message
            await Application.Current!.MainPage!.DisplayAlert(
                "Success",
                $"Appointment '{AppointmentTitle}' added to your calendar!",
                "OK");

            // Navigate back to calendar
            await Shell.Current.GoToAsync("calendar");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save appointment: {ex.Message}";
            Debug.WriteLine($"Confirm error: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void CloseResult()
    {
        ClearScanData();
        ShowResult = false;
    }

    private void ClearScanData()
    {
        Summary = string.Empty;
        ContainsAppointment = false;
        AppointmentTitle = string.Empty;
        AppointmentDate = DateOnly.FromDateTime(DateTime.Now);
        AppointmentTime = null;
        AppointmentLocation = string.Empty;
        AppointmentCategory = "Personal";
        ErrorMessage = string.Empty;
        _currentScanResult = null;
    }
}
