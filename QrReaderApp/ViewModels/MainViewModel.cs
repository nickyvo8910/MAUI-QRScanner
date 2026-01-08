using CommunityToolkit.Mvvm.Input;
using QrReaderApp.Views;

namespace QrReaderApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    public MainViewModel()
    {
        Title = "QR Code Reader";
    }

    [RelayCommand]
    private async Task NavigateToScanner()
    {
        await Shell.Current.GoToAsync(nameof(QrScannerPage));
    }
}
