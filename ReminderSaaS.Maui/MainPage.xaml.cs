using ReminderSaaS.Maui.ViewModels;

namespace ReminderSaaS.Maui;

public partial class MainPage : ContentPage
{
	int count = 0;
	MainShellViewModel _viewModel;

	public MainPage()
	{
		InitializeComponent();
		try
		{
			_viewModel = new MainShellViewModel();
			this.BindingContext = _viewModel;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error creating ViewModel: {ex.Message}");
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
