using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QrReaderApp.Services;
using ZXing.Net.Maui;

namespace QrReaderApp.ViewModels;

public partial class QrScannerViewModel : BaseViewModel
{
    private readonly IPermissionService _permissionService;
    private readonly IQrCodeService _qrCodeService;

    [ObservableProperty]
    private string scannedText = string.Empty;

    [ObservableProperty]
    private bool isScanning = true;

    [ObservableProperty]
    private bool hasPermission;

    public QrScannerViewModel(IPermissionService permissionService, IQrCodeService qrCodeService)
    {
        _permissionService = permissionService;
        _qrCodeService = qrCodeService;
        Title = "Scan QR Code";
    }

    [RelayCommand]
    private async Task CheckPermissions()
    {
        HasPermission = await _permissionService.CheckAndRequestCameraPermissionAsync();
        
        if (!HasPermission)
        {
            await Shell.Current.DisplayAlert(
                "Camera Permission",
                "Camera permission is required to scan QR codes. Please enable it in settings.",
                "OK");
        }
    }

    public void OnBarcodeDetected(BarcodeDetectionEventArgs args)
    {
        if (args.Results?.Length > 0)
        {
            var firstBarcode = args.Results[0];
            ScannedText = firstBarcode.Value;
            
            if (_qrCodeService.ValidateQrCode(ScannedText))
            {
                IsScanning = false;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "QR Code Scanned",
                        $"Content: {ScannedText}",
                        "OK");
                    
                    // Resume scanning after showing result
                    IsScanning = true;
                });
            }
        }
    }

    [RelayCommand]
    private void ToggleScanning()
    {
        IsScanning = !IsScanning;
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
