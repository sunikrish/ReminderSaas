using ReminderSaaS.Maui.ViewModels.Schedules;

namespace ReminderSaaS.Maui.Views.Schedules;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(CalendarViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CalendarPage constructor: {ex.Message}");
            throw;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            // Always refresh the calendar when page appears (including after navigation back)
            if (BindingContext is CalendarViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[CalendarPage.OnAppearing] Refreshing calendar data");
                await viewModel.InitializeCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CalendarPage OnAppearing: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load calendar: {ex.Message}", "OK");
        }
    }
}
