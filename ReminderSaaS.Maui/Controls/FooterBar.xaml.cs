namespace ReminderSaaS.Maui.Controls;

public partial class FooterBar : ContentView
{
	public static readonly BindableProperty CameraCommandProperty =
		BindableProperty.Create(nameof(CameraCommand), typeof(Command), typeof(FooterBar));

	public static readonly BindableProperty AddCommandProperty =
		BindableProperty.Create(nameof(AddCommand), typeof(Command), typeof(FooterBar));

	public static readonly BindableProperty VoiceCommandProperty =
		BindableProperty.Create(nameof(VoiceCommand), typeof(Command), typeof(FooterBar));

	public Command CameraCommand
	{
		get => (Command)GetValue(CameraCommandProperty);
		set => SetValue(CameraCommandProperty, value);
	}

	public Command AddCommand
	{
		get => (Command)GetValue(AddCommandProperty);
		set => SetValue(AddCommandProperty, value);
	}

	public Command VoiceCommand
	{
		get => (Command)GetValue(VoiceCommandProperty);
		set => SetValue(VoiceCommandProperty, value);
	}

	public FooterBar()
	{
		InitializeComponent();
		
		if (CameraButton != null)
			CameraButton.Clicked += (s, e) => CameraCommand?.Execute(null);
		
		if (AddButton != null)
			AddButton.Clicked += (s, e) => AddCommand?.Execute(null);
		
		if (VoiceButton != null)
			VoiceButton.Clicked += (s, e) => VoiceCommand?.Execute(null);
	}
}
