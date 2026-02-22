using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReminderSaaS.Shared.Contracts.Schedules;
using ReminderSaaS.Maui.Services;

namespace ReminderSaaS.Maui.ViewModels.Schedules;

/// <summary>
/// ViewModel for creating and editing schedules.
/// </summary>
public partial class ScheduleFormViewModel : ObservableObject
{
    private readonly IScheduleApiClient _apiClient;
    private Guid? _editingScheduleId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private DateOnly selectedDate;

    [ObservableProperty]
    private TimeSpan? selectedTime;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "Personal";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    private readonly string[] _categories = { "Government", "Kids School", "Personal", "Health", "Car", "Finance" };

    public IReadOnlyList<string> Categories => _categories;

    public ScheduleFormViewModel(IScheduleApiClient apiClient)
    {
        _apiClient = apiClient;
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
    }

    /// <summary>
    /// Initializes the form for creating a new schedule.
    /// </summary>
    [RelayCommand]
    public void InitializeForCreate(DateOnly? date = null)
    {
        _editingScheduleId = null;
        Clear();
        if (date.HasValue)
        {
            SelectedDate = date.Value;
        }
    }

    /// <summary>
    /// Initializes the form for editing an existing schedule.
    /// </summary>
    [RelayCommand]
    public async Task InitializeForEditAsync(Guid scheduleId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var schedule = await _apiClient.GetScheduleByIdAsync(scheduleId);
            if (schedule != null)
            {
                _editingScheduleId = schedule.Id;
                Title = schedule.Title;
                Description = schedule.Description ?? string.Empty;
                SelectedDate = schedule.Date;
                SelectedTime = schedule.Time?.ToTimeSpan();
                Location = schedule.Location ?? string.Empty;
                SelectedCategory = schedule.Category;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load schedule details";
            System.Diagnostics.Debug.WriteLine($"Load schedule error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves the schedule (creates or updates).
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                ErrorMessage = "Category is required";
                return;
            }

            if (SelectedDate == default(DateOnly))
            {
                ErrorMessage = "Schedule date is required";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            if (_editingScheduleId.HasValue)
            {
                // Update existing schedule
                var updateDto = new UpdateScheduleDto(
                    _editingScheduleId.Value,
                    Title,
                    SelectedDate,
                    SelectedCategory,
                    string.IsNullOrWhiteSpace(Description) ? null : Description,
                    SelectedTime.HasValue ? TimeOnly.FromTimeSpan(SelectedTime.Value) : null,
                    string.IsNullOrWhiteSpace(Location) ? null : Location);

                await _apiClient.UpdateScheduleAsync(updateDto);
            }
            else
            {
                // Create new schedule
                var createDto = new CreateScheduleDto(
                    Title,
                    SelectedDate,
                    SelectedCategory,
                    string.IsNullOrWhiteSpace(Description) ? null : Description,
                    SelectedTime.HasValue ? TimeOnly.FromTimeSpan(SelectedTime.Value) : null,
                    string.IsNullOrWhiteSpace(Location) ? null : Location);

                var newScheduleId = await _apiClient.CreateScheduleAsync(createDto);
            }

            // Show success confirmation message
            string message = _editingScheduleId.HasValue ? "Schedule updated successfully!" : "Schedule created successfully!";
            await Application.Current!.MainPage!.DisplayAlert("Success", message, "OK");

            // Navigate back to calendar. Try modal/pop navigation first, fallback to absolute shell route.
            try
            {
                if (Shell.Current.Navigation.ModalStack?.Count > 0)
                {
                    await Shell.Current.Navigation.PopModalAsync();
                }
                else if (Shell.Current.Navigation.NavigationStack?.Count > 1)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync("//calendar");
                }
            }
            catch (Exception exNav)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleFormViewModel.SaveAsync] Navigation error: {exNav.Message}\n{exNav.StackTrace}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save schedule: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to save schedule: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[ScheduleFormViewModel.SaveAsync] Error: {ex.Message}\nStack: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Cancels the form and navigates back to calendar.
    /// </summary>
    [RelayCommand]
    public async Task CancelAsync()
    {
        try
        {
            // Try modal pop first (if opened modally)
            if (Shell.Current.Navigation.ModalStack?.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
                return;
            }

            // Try normal pop
            if (Shell.Current.Navigation.NavigationStack?.Count > 1)
            {
                await Shell.Current.Navigation.PopAsync();
                return;
            }

            // Fallback to absolute route to calendar
            await Shell.Current.GoToAsync("//calendar");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleFormViewModel.CancelAsync] Navigation error: {ex.Message}\nStack: {ex.StackTrace}");
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Clears all form fields.
    /// </summary>
    private void Clear()
    {
        Title = string.Empty;
        Description = string.Empty;
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        SelectedTime = null;
        Location = string.Empty;
        SelectedCategory = "Personal";
        ErrorMessage = string.Empty;
    }
}
