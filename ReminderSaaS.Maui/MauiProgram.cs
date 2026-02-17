using Microsoft.Extensions.Logging;
using ReminderSaaS.Maui.Services;
using ReminderSaaS.Maui.ViewModels.Schedules;
using ReminderSaaS.Maui.Views.Schedules;

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
			});

		// Register HttpClient for Schedule API
		var baseUrl = "http://localhost:7071"; // Local development
		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			baseUrl = "http://10.0.2.2:7071"; // Android emulator
		}

		builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseUrl) });
		builder.Services.AddSingleton<IScheduleApiClient>(sp => new ScheduleApiClient(sp.GetRequiredService<HttpClient>()));

		// Register ViewModels
		builder.Services.AddSingleton<CalendarViewModel>();
		builder.Services.AddTransient<ScheduleFormViewModel>(); // Transient to get fresh state for each edit
		builder.Services.AddTransient<ScheduleDetailViewModel>(); // Transient to get fresh state for each schedule

		// Register Views
		builder.Services.AddSingleton<CalendarPage>();
		builder.Services.AddTransient<ScheduleFormPage>(); // Transient to work with fresh ViewModel
		builder.Services.AddTransient<ScheduleDetailPage>(); // Transient to work with fresh ViewModel

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
