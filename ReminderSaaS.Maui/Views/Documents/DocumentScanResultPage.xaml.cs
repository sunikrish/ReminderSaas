using System.Diagnostics;
using ReminderSaaS.Maui.ViewModels.Documents;

namespace ReminderSaaS.Maui.Views.Documents;

public partial class DocumentScanResultPage : ContentPage
{
	public DocumentScanResultPage(DocumentScanViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		Debug.WriteLine("DocumentScanResultPage: constructed and BindingContext set");
	}
}
