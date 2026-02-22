using Microsoft.Extensions.DependencyInjection;
using ReminderSaaS.Maui.ViewModels;
using ReminderSaaS.Maui.Views.Schedules;
using ReminderSaaS.Maui.Views.Documents;

namespace ReminderSaaS.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// Resolve the shell ViewModel from DI and set as BindingContext
		var services = Application.Current?.Handler?.MauiContext?.Services;
		if (services != null)
		{
			this.BindingContext = services.GetRequiredService<MainShellViewModel>();
		}
		else
		{
			// Fallback for design-time or unexpected runtime: create with the service provider
			this.BindingContext = new MainShellViewModel(services!);
		}

		// Register routes for navigation with query parameters
		Routing.RegisterRoute("scheduleform", typeof(ScheduleFormPage));
		Routing.RegisterRoute("scheduledetail", typeof(ScheduleDetailPage));
		Routing.RegisterRoute("documentscan", typeof(DocumentScanResultPage));
	}
}
