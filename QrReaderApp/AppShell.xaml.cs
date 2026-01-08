using QrReaderApp.Views;

namespace QrReaderApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute(nameof(QrScannerPage), typeof(QrScannerPage));
	}
}
