using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace ReminderSaaS.Maui.ViewModels;

/// <summary>
/// ViewModel for the MainShell (Footer) - shared across all pages
/// Handles navigation and actions from the global footer
/// </summary>
public partial class MainShellViewModel : ObservableObject
{
    public ICommand CameraCommand { get; }
    public ICommand AddScheduleCommand { get; }
    public ICommand VoiceCommand { get; }

    public MainShellViewModel()
    {
        CameraCommand = new AsyncRelayCommand(OnCameraClickedAsync);
        AddScheduleCommand = new AsyncRelayCommand(OnAddScheduleClickedAsync);
        VoiceCommand = new AsyncRelayCommand(OnVoiceClickedAsync);
    }

    private async Task OnCameraClickedAsync()
    {
        try
        {
            Debug.WriteLine("Camera command executed - navigating to document scan");
            await Shell.Current.GoToAsync("documentscan");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Camera navigation error: {ex.Message}");
        }
    }

    private async Task OnAddScheduleClickedAsync()
    {
        try
        {
            Debug.WriteLine("Add Schedule command executed - navigating to schedule form");
            await Shell.Current.GoToAsync("scheduleform");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private async Task OnVoiceClickedAsync()
    {
        Debug.WriteLine("Voice command executed");
        // TODO: Implement voice/speaker functionality
        await Task.CompletedTask;
    }
}
