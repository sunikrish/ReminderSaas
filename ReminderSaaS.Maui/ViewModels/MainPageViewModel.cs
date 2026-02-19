using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReminderSaaS.Maui.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public ObservableCollection<string> Reminders { get; } = new ObservableCollection<string>();

    [RelayCommand]
    private async Task AddReminderAsync()
    {
        // Placeholder for calling your Backend Function
        Reminders.Add("New Reminder at " + DateTime.Now);
    }
}