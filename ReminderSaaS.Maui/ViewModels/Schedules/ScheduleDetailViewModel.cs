using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReminderSaaS.Shared.Contracts.Schedules;
using ReminderSaaS.Maui.Services;

namespace ReminderSaaS.Maui.ViewModels.Schedules;

/// <summary>
/// ViewModel for viewing schedule details.
/// </summary>
public partial class ScheduleDetailViewModel : ObservableObject
{
    private readonly IScheduleApiClient _apiClient;

    [ObservableProperty]
    private ScheduleDto? schedule;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ScheduleDetailViewModel(IScheduleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Loads the schedule details by ID.
    /// </summary>
    [RelayCommand]
    public async Task LoadScheduleAsync(Guid scheduleId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Schedule = await _apiClient.GetScheduleByIdAsync(scheduleId);

            if (Schedule == null)
            {
                ErrorMessage = "Schedule not found";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load schedule details";
            System.Diagnostics.Debug.WriteLine($"[ScheduleDetailViewModel] Load schedule error: {ex.Message}\nStack: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates to edit the schedule.
    /// </summary>
    [RelayCommand]
    public async Task EditScheduleAsync()
    {
        if (Schedule != null)
        {
            await Shell.Current.GoToAsync($"scheduleform?id={Schedule.Id}");
        }
    }

    /// <summary>
    /// Deletes the schedule with confirmation.
    /// </summary>
    [RelayCommand]
    public async Task DeleteScheduleAsync()
    {
        if (Schedule == null)
            return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Delete Schedule",
            $"Are you sure you want to delete '{Schedule.Title}'?",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        try
        {
            IsLoading = true;
            await _apiClient.DeleteScheduleAsync(Schedule.Id);

            // Navigate back to calendar
            await Application.Current!.MainPage!.DisplayAlert("Success", "Schedule deleted successfully!", "OK");
            await Shell.Current.GoToAsync("calendar");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to delete schedule";
            System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates back to calendar.
    /// </summary>
    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("calendar");
    }

    /// <summary>
    /// Gets the category color for display.
    /// </summary>
    public string GetCategoryColor() => Schedule?.Category switch
    {
        "Health" => "#FF6B6B",
        "Finance" => "#4ECDC4",
        "School" => "#45B7D1",
        "Kids School" => "#45B7D1",
        "KidsSchool" => "#45B7D1",
        "Government" => "#45B7D1",
        "Personal" => "#FFA07A",
        "Other" => "#95E1D3",
        _ => "#808080"
    };

    /// <summary>
    /// Gets the formatted date and time string.
    /// </summary>
    public string GetFormattedDateTime()
    {
        if (Schedule == null)
            return string.Empty;

        var dateStr = Schedule.Date.ToString("dddd, MMMM d, yyyy");
        if (Schedule.Time.HasValue)
        {
            return $"{dateStr} at {Schedule.Time:hh:mm tt}";
        }

        return dateStr;
    }
}
