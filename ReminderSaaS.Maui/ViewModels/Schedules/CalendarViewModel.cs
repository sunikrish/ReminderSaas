using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReminderSaaS.Shared.Contracts.Schedules;
using ReminderSaaS.Maui.Services;

namespace ReminderSaaS.Maui.ViewModels.Schedules;

/// <summary>
/// Represents a single day cell in the calendar.
/// </summary>
public partial class CalendarDayCell : ObservableObject
{
    [ObservableProperty]
    private DateOnly? date;

    [ObservableProperty]
    private int dayNumber;

    [ObservableProperty]
    private bool isCurrentMonth;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isToday;

    [ObservableProperty]
    private ObservableCollection<ScheduleDto> schedules = new();

    [ObservableProperty]
    private Color borderColor = Color.FromArgb("#E0E0E0");

    [ObservableProperty]
    private Color backgroundColor = Color.FromArgb("#FFFFFF");

    [ObservableProperty]
    private Color textColor = Color.FromArgb("#000000");

    public bool HasSchedules => Schedules.Count > 0;

    /// <summary>
    /// Constructor initializes collection change listener.
    /// </summary>
    public CalendarDayCell()
    {
        Schedules.CollectionChanged += (s, e) => UpdateColors();
    }

    /// <summary>
    /// Updates all color properties based on current state.
    /// </summary>
    public void UpdateColors()
    {
        if (IsToday)
        {
            BorderColor = Color.FromArgb("#FF6B6B");      // Red for today
            BackgroundColor = Color.FromArgb("#FFE5E5");  // Light red for today
            TextColor = Color.FromArgb("#FF6B6B");        // Red text for today
        }
        else if (HasSchedules)
        {
            BorderColor = Color.FromArgb("#4CAF50");      // Green for events
            BackgroundColor = Color.FromArgb("#E8F5E9");  // Light green for events
            TextColor = Color.FromArgb("#2E7D32");        // Dark green text for events
        }
        else
        {
            BorderColor = Color.FromArgb("#E0E0E0");      // Gray default
            BackgroundColor = Color.FromArgb("#FFFFFF");  // White default
            TextColor = Color.FromArgb("#000000");        // Black default
        }
    }

    /// <summary>
    /// Handles property changes to update dependent color properties.
    /// </summary>
    partial void OnSchedulesChanged(ObservableCollection<ScheduleDto> value)
    {
        OnPropertyChanged(nameof(HasSchedules));
        OnPropertyChanged(nameof(BorderColor));
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
    }

    /// <summary>
    /// <summary>
    /// Handles changes to IsToday to update color properties.
    /// </summary>
    partial void OnIsTodayChanged(bool value)
    {
        UpdateColors();
    }

    /// <summary>
    /// Adds a schedule and updates colors.
    /// </summary>
    public void AddScheduleWithNotification(ScheduleDto schedule)
    {
        Schedules.Add(schedule);
        UpdateColors();
    }
}

/// <summary>
/// Represents a week row in the calendar.
/// </summary>
public class CalendarWeek
{
    public List<CalendarDayCell> Days { get; set; } = new();
}

/// <summary>
/// ViewModel for the Calendar view displaying monthly schedules.
/// </summary>
public partial class CalendarViewModel : ObservableObject
{
    private readonly IScheduleApiClient _apiClient;
    private Dictionary<DateOnly, List<ScheduleDto>> _allSchedules = new();

    [ObservableProperty]
    private int currentYear;

    [ObservableProperty]
    private int currentMonth;

    [ObservableProperty]
    private DateOnly selectedDate;

    [ObservableProperty]
    private ObservableCollection<ScheduleDto> schedulesForSelectedDate = new();

    [ObservableProperty]
    private ObservableCollection<DateOnly> daysInCurrentMonth = new();

    [ObservableProperty]
    private ObservableCollection<CalendarWeek> calendarWeeks = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string monthYearDisplay = string.Empty;

    public CalendarViewModel(IScheduleApiClient apiClient)
    {
        _apiClient = apiClient;
        var today = DateOnly.FromDateTime(DateTime.Now);
        CurrentYear = today.Year;
        CurrentMonth = today.Month;
        SelectedDate = today;
        UpdateMonthYearDisplay();
    }

    private void UpdateMonthYearDisplay()
    {
        var date = new DateOnly(CurrentYear, CurrentMonth, 1);
        MonthYearDisplay = date.ToString("MMMM yyyy");
    }

    /// <summary>
    /// Initializes the calendar and loads schedules for the current month.
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // CRITICAL: Load schedules FIRST, which will also call GenerateDaysInMonth
            // This ensures _allSchedules is populated BEFORE calendar day cells are created
            await LoadSchedulesForMonthAsync();
            // LoadSchedulesForMonthAsync calls GenerateDaysInMonth internally
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to initialize calendar";
            System.Diagnostics.Debug.WriteLine($"[InitializeAsync] Calendar initialization error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates to the previous month.
    /// </summary>
    [RelayCommand]
    public async Task PreviousMonthAsync()
    {
        CurrentMonth--;
        if (CurrentMonth < 1)
        {
            CurrentMonth = 12;
            CurrentYear--;
        }

        UpdateMonthYearDisplay();
        // Select first day of month
        SelectedDate = new DateOnly(CurrentYear, CurrentMonth, 1);
        await LoadSchedulesForMonthAsync();
    }

    /// <summary>
    /// Navigates to the next month.
    /// </summary>
    [RelayCommand]
    public async Task NextMonthAsync()
    {
        CurrentMonth++;
        if (CurrentMonth > 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }

        UpdateMonthYearDisplay();
        // Select first day of month
        SelectedDate = new DateOnly(CurrentYear, CurrentMonth, 1);
        await LoadSchedulesForMonthAsync();
    }

    /// <summary>
    /// Handles date selection on the calendar.
    /// </summary>
    [RelayCommand]
    public void SelectDate(CalendarDayCell dayCell)
    {
        if (dayCell?.Date == null)
            return;

        // Update selected date
        SelectedDate = dayCell.Date.Value;

        // Update UI - clear previous selection and mark new one as selected
        foreach (var week in CalendarWeeks)
        {
            foreach (var day in week.Days)
            {
                day.IsSelected = day.Date == SelectedDate;
            }
        }

        FilterSchedulesForSelectedDate();
    }

    /// <summary>
    /// Refreshes the current month's schedules.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadSchedulesForMonthAsync();
    }

    /// <summary>
    /// Navigates to create a new schedule for the selected date.
    /// </summary>
    [RelayCommand]
    public async Task CreateScheduleAsync()
    {
        await Shell.Current.GoToAsync($"scheduleform?date={SelectedDate:yyyy-MM-dd}");
    }

    /// <summary>
    /// Navigates to edit an existing schedule.
    /// </summary>
    [RelayCommand]
    public async Task EditScheduleAsync(ScheduleDto schedule)
    {
        if (schedule == null)
        {
            return;
        }

        try
        {
            var navigationUri = $"///scheduledetail?scheduleid={schedule.Id}";
            await Shell.Current.GoToAsync(navigationUri);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CalendarViewModel.EditScheduleAsync] Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a schedule.
    /// </summary>
    [RelayCommand]
    public async Task DeleteScheduleAsync(ScheduleDto schedule)
    {
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Delete Schedule",
            $"Are you sure you want to delete '{schedule.Title}'?",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        try
        {
            IsLoading = true;
            await _apiClient.DeleteScheduleAsync(schedule.Id);
            await RefreshAsync();
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
    /// Loads all schedules for the current month from the API.
    /// </summary>
    private async Task LoadSchedulesForMonthAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var schedules = await _apiClient.GetSchedulesByMonthAsync(CurrentYear, CurrentMonth);
            
            _allSchedules.Clear();
            if (schedules != null && schedules.Count > 0)
            {
                foreach (var schedule in schedules)
                {
                    if (!_allSchedules.ContainsKey(schedule.Date))
                    {
                        _allSchedules[schedule.Date] = new List<ScheduleDto>();
                    }
                    _allSchedules[schedule.Date].Add(schedule);
                }
            }
            
            // Regenerate calendar with updated schedules
            GenerateDaysInMonth(CurrentYear, CurrentMonth);
            FilterSchedulesForSelectedDate();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load schedules";
            System.Diagnostics.Debug.WriteLine($"[CalendarViewModel] Load schedules error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Filters schedules to show only those matching the selected date.
    /// </summary>
    private void FilterSchedulesForSelectedDate()
    {
        SchedulesForSelectedDate.Clear();
        if (SelectedDate != default && _allSchedules.TryGetValue(SelectedDate, out var schedules))
        {
            foreach (var schedule in schedules.OrderBy(s => s.Time))
            {
                SchedulesForSelectedDate.Add(schedule);
            }
        }
    }

    /// <summary>
    /// Generates the list of days for the given month, organized into weeks.
    /// </summary>
    private void GenerateDaysInMonth(int year, int month)
    {
        System.Diagnostics.Debug.WriteLine($"\n[GenerateDaysInMonth] Starting calendar generation for {year}-{month}");
        System.Diagnostics.Debug.WriteLine($"[GenerateDaysInMonth] Available schedule dates: {string.Join(", ", _allSchedules.Keys.OrderBy(d => d))}");
        
        DaysInCurrentMonth.Clear();
        CalendarWeeks.Clear();

        var today = DateOnly.FromDateTime(DateTime.Now);
        System.Diagnostics.Debug.WriteLine($"[GenerateDaysInMonth] Today is: {today}");
        
        var firstDay = new DateOnly(year, month, 1);
        int daysInMonth = DateTime.DaysInMonth(year, month);
        int firstDayOfWeek = (int)firstDay.DayOfWeek;

        var week = new CalendarWeek();

        // Add empty cells for days before the first of the month
        for (int i = 0; i < firstDayOfWeek; i++)
        {
            week.Days.Add(new CalendarDayCell
            {
                Date = null,
                DayNumber = 0,
                IsCurrentMonth = false,
                IsToday = false
            });
        }

        // Add days of the current month
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            DaysInCurrentMonth.Add(date);

            var dayCell = new CalendarDayCell
            {
                Date = date,
                DayNumber = day,
                IsCurrentMonth = true,
                IsSelected = date == SelectedDate,
                IsToday = date == today
            };

            // Add schedules for this day if they exist
            if (_allSchedules.TryGetValue(date, out var schedules))
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateDaysInMonth] ✓ Day {day}: Found {schedules.Count} schedules");
                foreach (var schedule in schedules)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateDaysInMonth]   - Adding: '{schedule.Title}'");
                    dayCell.AddScheduleWithNotification(schedule);
                }
            }
            else
            {
                // Even if no schedules, need to update colors based on IsToday
                System.Diagnostics.Debug.WriteLine($"[GenerateDaysInMonth] Day {day}: No schedules, IsToday={dayCell.IsToday}, UpdateColors called");
                dayCell.UpdateColors();
            }

            week.Days.Add(dayCell);

            if (week.Days.Count == 7)
            {
                CalendarWeeks.Add(week);
                week = new CalendarWeek();
            }
        }

        // Add empty cells to complete the last week
        if (week.Days.Count > 0)
        {
            while (week.Days.Count < 7)
            {
                week.Days.Add(new CalendarDayCell
                {
                    Date = null,
                    DayNumber = 0,
                    IsCurrentMonth = false
                });
            }
            CalendarWeeks.Add(week);
        }
    }
}
