using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReminderSaaS.Maui.Services;
using ReminderSaaS.Shared.Contracts.Schedules;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

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

            // Try to close modal or navigate back to calendar
            try
            {
                if (Shell.Current.Navigation.ModalStack?.Count > 0)
                {
                    await Shell.Current.Navigation.PopModalAsync(true);
                }
                else if (Shell.Current.Navigation.NavigationStack?.Count > 1)
                {
                    await Shell.Current.Navigation.PopAsync(true);
                }
                else
                {
                    await Shell.Current.GoToAsync("//calendar", true);
                }
            }
            catch
            {
                try { await Shell.Current.GoToAsync("//calendar", true); } catch { }
            }
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
    public async Task CloseResultAsync()
    {
        Debug.WriteLine("CloseResult invoked");
        ClearScanData();
        ShowResult = false;

        try
        {
            // Pop the modal on the main thread to avoid window/drawing race conditions
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // If this page was presented modally, pop the modal
                if (Shell.Current.Navigation.ModalStack?.Count > 0)
                {
                    await Shell.Current.Navigation.PopModalAsync(true);
                    return;
                }

                // Otherwise attempt a normal navigation pop (if possible)
                if (Shell.Current.Navigation.NavigationStack?.Count > 1)
                {
                    await Shell.Current.Navigation.PopAsync(true);
                    return;
                }

                // As a last resort try Shell navigation to go up one level
                try
                {
                    await Shell.Current.GoToAsync("..", true);
                }
                catch
                {
                    // ignore failures silently; nothing more we can do
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error closing scan modal: {ex.Message}");
        }
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
