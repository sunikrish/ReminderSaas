using ReminderSaaS.Maui.ViewModels.Schedules;

namespace ReminderSaaS.Maui.Views.Schedules;

public partial class CalendarPage : ContentPage
{
    private bool _isInitialized = false;

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
            if (!_isInitialized && BindingContext is CalendarViewModel viewModel)
            {
                _isInitialized = true;
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
