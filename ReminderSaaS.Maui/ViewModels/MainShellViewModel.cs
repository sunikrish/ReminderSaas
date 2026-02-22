using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ReminderSaaS.Maui.Views.Documents;
using ReminderSaaS.Maui.ViewModels.Documents;
using Microsoft.Extensions.DependencyInjection;

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

    private readonly IServiceProvider _services;

    public MainShellViewModel(IServiceProvider services)
    {
        _services = services;
        CameraCommand = new AsyncRelayCommand(OnCameraClickedAsync);
        AddScheduleCommand = new AsyncRelayCommand(OnAddScheduleClickedAsync);
        VoiceCommand = new AsyncRelayCommand(OnVoiceClickedAsync);
    }

    private async Task OnCameraClickedAsync()
    {
        try
        {
            Debug.WriteLine("Camera command executed - opening scan modal");

            // Resolve a transient DocumentScanResultPage, push it modally and start scanning
            var page = _services.GetService(typeof(DocumentScanResultPage)) as DocumentScanResultPage;
            if (page == null)
            {
                Debug.WriteLine("Failed to resolve DocumentScanResultPage from DI.");
                return;
            }

            // Show the scan page as a modal so user stays in context
            await Shell.Current.Navigation.PushModalAsync(page);

            // Trigger scan on the page's ViewModel (if available)
            if (page.BindingContext is ReminderSaaS.Maui.ViewModels.Documents.DocumentScanViewModel vm)
            {
                await vm.ScanDocumentAsync();
            }
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
