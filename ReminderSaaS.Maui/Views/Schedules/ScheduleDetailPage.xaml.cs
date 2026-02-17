using ReminderSaaS.Maui.ViewModels.Schedules;

namespace ReminderSaaS.Maui.Views.Schedules;

[QueryProperty(nameof(ScheduleId), "scheduleid")]
public partial class ScheduleDetailPage : ContentPage
{
    private string? _scheduleId;
    public string? ScheduleId
    {
        get => _scheduleId;
        set
        {
            _scheduleId = Uri.UnescapeDataString(value ?? string.Empty);
            System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage.QueryProperty] Received ScheduleId: {_scheduleId}");
            
            // Trigger load immediately when ScheduleId is set
            if (BindingContext is ScheduleDetailViewModel viewModel && !string.IsNullOrEmpty(_scheduleId))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage.QueryProperty] LoadScheduleDataAsync triggered from property setter");
                _ = LoadScheduleDataAsync(viewModel);
            }
        }
    }

    public ScheduleDetailPage(ScheduleDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage.ctor] Page initialized");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage.OnAppearing] Called, ScheduleId: {ScheduleId}");
    }

    private async Task LoadScheduleDataAsync(ScheduleDetailViewModel viewModel)
    {
        System.Diagnostics.Debug.WriteLine($"\n[ScheduleDetailPage.LoadScheduleDataAsync] ===== LOADING SCHEDULE DETAILS =====");
        try
        {
            if (viewModel == null!)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] ViewModel is null! Cannot load schedule.");
                return;
            }

            if (string.IsNullOrEmpty(ScheduleId))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] ScheduleId is empty! Cannot load schedule.");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] ScheduleId from property: {ScheduleId}");

            if (Guid.TryParse(ScheduleId, out var scheduleId))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Parsed GUID successfully: {scheduleId}");
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] About to call LoadScheduleAsync with ID: {scheduleId}");
               
                // NOW properly await the async call
                await viewModel.LoadScheduleAsync(scheduleId);
                
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] LoadScheduleAsync completed");
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Schedule after load: {viewModel.Schedule?.Title}");
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Schedule ID after load: {viewModel.Schedule?.Id}");
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Schedule is null: {viewModel.Schedule == null}");
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] ErrorMessage: {viewModel.ErrorMessage}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Failed to parse GUID: {ScheduleId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] Error in LoadScheduleDataAsync: {ex.Message}\nStack: {ex.StackTrace}");
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleDetailPage] ===== FINISHED LOADING SCHEDULE DETAILS =====");
        }
    }
}


