using Microsoft.Extensions.Logging;
using ReminderSaaS.Maui.Services;
using ReminderSaaS.Maui.ViewModels;
using ReminderSaaS.Maui.ViewModels.Schedules;
using ReminderSaaS.Maui.Views.Schedules;
using ReminderSaaS.Maui.ViewModels.Documents;
using ReminderSaaS.Maui.Views.Documents;

namespace ReminderSaaS.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
			});

		// Register HttpClient for Schedule API (use Functions host port 7147)
		var baseUrl = "http://localhost:7147"; // Local development (Functions host)
		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			baseUrl = "http://10.0.2.2:7147"; // Android emulator -> host machine
		}

		builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseUrl) });
		builder.Services.AddSingleton<IScheduleApiClient>(sp => new ScheduleApiClient(sp.GetRequiredService<HttpClient>()));

		// Register Document Scan Service
		builder.Services.AddSingleton<IDocumentScanService>(sp => new DocumentScanService(sp.GetRequiredService<HttpClient>()));

		// Register Shell ViewModels
		builder.Services.AddSingleton<MainShellViewModel>();

		// Register ViewModels
		builder.Services.AddSingleton<CalendarViewModel>();
		builder.Services.AddTransient<ScheduleFormViewModel>(); // Transient to get fresh state for each edit
		builder.Services.AddTransient<ScheduleDetailViewModel>(); // Transient to get fresh state for each schedule
		builder.Services.AddTransient<DocumentScanViewModel>(); // Transient for document scanning

		// Register Views
		builder.Services.AddSingleton<CalendarPage>();
		builder.Services.AddTransient<ScheduleFormPage>(); // Transient to work with fresh ViewModel
		builder.Services.AddTransient<ScheduleDetailPage>(); // Transient to work with fresh ViewModel
		builder.Services.AddTransient<DocumentScanResultPage>(); // Transient for document scan results

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
