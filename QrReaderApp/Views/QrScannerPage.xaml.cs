using QrReaderApp.ViewModels;
using ZXing.Net.Maui;

namespace QrReaderApp.Views;

public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;

    public QrScannerPage(QrScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckPermissionsCommand.ExecuteAsync(null);
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            _viewModel.OnBarcodeDetected(e);
        });
    }
}
