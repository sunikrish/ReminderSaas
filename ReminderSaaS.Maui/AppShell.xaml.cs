using ReminderSaaS.Maui.ViewModels;
using ReminderSaaS.Maui.Views.Schedules;
using ReminderSaaS.Maui.Views.Documents;

namespace ReminderSaaS.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// Set the DataContext for Shell-level binding (Footer)
		this.BindingContext = new MainShellViewModel();

		// Register routes for navigation with query parameters
		Routing.RegisterRoute("scheduleform", typeof(ScheduleFormPage));
		Routing.RegisterRoute("scheduledetail", typeof(ScheduleDetailPage));
		Routing.RegisterRoute("documentscan", typeof(DocumentScanResultPage));
	}
}
