using Microsoft.Extensions.Logging;
using QrReaderApp.Services;
using QrReaderApp.ViewModels;
using QrReaderApp.Views;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;

namespace QrReaderApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Services
		builder.Services.AddSingleton<IPermissionService, PermissionService>();
		builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

		// Register ViewModels
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddTransient<QrScannerViewModel>();

		// Register Views
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<QrScannerPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
