using ReminderSaaS.Maui.ViewModels.Schedules;

namespace ReminderSaaS.Maui.Views.Schedules;

[QueryProperty(nameof(ScheduleId), "id")]
[QueryProperty(nameof(ScheduleDate), "date")]
public partial class ScheduleFormPage : ContentPage
{
    private string? _scheduleId;
    public string? ScheduleId
    {
        get => _scheduleId;
        set
        {
            _scheduleId = Uri.UnescapeDataString(value ?? string.Empty);
            System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage.QueryProperty] Received ScheduleId: {_scheduleId}");
            TriggerInitialize();
        }
    }

    private string? _scheduleDate;
    public string? ScheduleDate
    {
        get => _scheduleDate;
        set
        {
            _scheduleDate = Uri.UnescapeDataString(value ?? string.Empty);
            System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage.QueryProperty] Received ScheduleDate: {_scheduleDate}");
            TriggerInitialize();
        }
    }

    public ScheduleFormPage(ScheduleFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage.ctor] Page initialized");
    }

    private void TriggerInitialize()
    {
        if (BindingContext is ScheduleFormViewModel viewModel)
        {
            // If editing (id parameter provided)
            if (!string.IsNullOrEmpty(ScheduleId) && Guid.TryParse(ScheduleId, out var scheduleId))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage] Initializing for EDIT with ID: {scheduleId}");
                _ = viewModel.InitializeForEditAsync(scheduleId);
            }
            // If creating (date parameter provided)
            else if (!string.IsNullOrEmpty(ScheduleDate) && DateOnly.TryParse(ScheduleDate, out var date))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage] Initializing for CREATE with date: {date}");
                viewModel.InitializeForCreate(date);
            }
            // Default: create with today's date
            else if (string.IsNullOrEmpty(ScheduleId) && string.IsNullOrEmpty(ScheduleDate))
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage] Initializing for CREATE with TODAY");
                viewModel.InitializeForCreate();
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"[ScheduleFormPage.OnAppearing] Called, ScheduleId: {ScheduleId}, ScheduleDate: {ScheduleDate}");
    }
}
