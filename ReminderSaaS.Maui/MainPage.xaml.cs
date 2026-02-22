using Microsoft.Extensions.DependencyInjection;
using ReminderSaaS.Maui.ViewModels;

namespace ReminderSaaS.Maui;

public partial class MainPage : ContentPage
{
	int count = 0;
	MainShellViewModel _viewModel;

	public MainPage()
	{
		InitializeComponent();
		var services = Application.Current?.Handler?.MauiContext?.Services;
		if (services != null)
		{
			_viewModel = services.GetRequiredService<MainShellViewModel>();
			this.BindingContext = _viewModel;
		}
		else
		{
			// Fallback: construct with null service provider in unlikely case services are unavailable
			_viewModel = new MainShellViewModel(null!);
			this.BindingContext = _viewModel;
		}
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private void OnCameraClicked(object sender, EventArgs e)
	{
		_viewModel?.CameraCommand?.Execute(null);
	}

	private void OnAddClicked(object sender, EventArgs e)
	{
		_viewModel?.AddScheduleCommand?.Execute(null);
	}

	private void OnVoiceClicked(object sender, EventArgs e)
	{
		_viewModel?.VoiceCommand?.Execute(null);
	}
}
