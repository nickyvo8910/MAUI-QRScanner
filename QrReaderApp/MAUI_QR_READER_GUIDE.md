# .NET MAUI QR Code Reader - Architecture Guide

## Overview
This guide provides a modern architecture for building a .NET MAUI QR code reader app, tailored for developers with Xamarin Forms + MVVMCross experience.

## Architecture Recommendation: MVVM + CommunityToolkit.Mvvm

### Why CommunityToolkit.Mvvm?
- **Official Microsoft recommendation** replacing MVVMCross/Prism for MAUI
- **Source generators** for reduced boilerplate (no more `RaisePropertyChanged`)
- **Built-in DI integration** with Microsoft.Extensions.DependencyInjection
- **Lightweight** and performant
- **Async commands** out of the box

### Key Differences from MVVMCross

| MVVMCross | .NET MAUI + CommunityToolkit |
|-----------|------------------------------|
| `MvxViewModel` | `ObservableObject` |
| `RaisePropertyChanged()` | `[ObservableProperty]` attribute |
| `MvxCommand` | `[RelayCommand]` attribute |
| `MvxIoCProvider` | `Microsoft.Extensions.DependencyInjection` |
| Custom navigation | `INavigationService` or Shell navigation |
| Platform-specific code in separate projects | Platform folders in single project + `#if` directives |

## Recommended Project Structure

```
YourApp/
├── App.xaml / App.xaml.cs          # App entry point
├── AppShell.xaml / AppShell.cs     # Shell navigation
├── MauiProgram.cs                  # DI container setup
│
├── Models/                          # Data models
│   └── QrScanResult.cs
│
├── ViewModels/                      # ViewModels (MVVM)
│   ├── BaseViewModel.cs
│   ├── MainViewModel.cs
│   └── QrScannerViewModel.cs
│
├── Views/                           # XAML pages
│   ├── MainPage.xaml
│   └── QrScannerPage.xaml
│
├── Services/                        # Business logic & interfaces
│   ├── Interfaces/
│   │   ├── IQrCodeService.cs
│   │   └── IPermissionService.cs
│   └── Implementations/
│       ├── QrCodeService.cs
│       └── PermissionService.cs
│
├── Platforms/                       # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
│
└── Resources/                       # Images, fonts, styles
    ├── Images/
    ├── Fonts/
    └── Styles/
```

## QR Code Library Recommendation

### Option 1: **ZXing.Net.Maui** (Recommended)
- Actively maintained for MAUI
- Cross-platform (Android, iOS, Windows, Mac)
- Easy to use
- Good performance

```bash
dotnet add package ZXing.Net.Maui
dotnet add package ZXing.Net.Maui.Controls
```

### Option 2: **BarcodeScanner.Mobile.Maui**
- Alternative with similar features
- Based on ML Kit (Android) and Vision (iOS)

## Step-by-Step Implementation Guide

### 1. Create New MAUI Project

```bash
dotnet new maui -n QrReaderApp
cd QrReaderApp
```

### 2. Install Required NuGet Packages

```bash
# MVVM Toolkit
dotnet add package CommunityToolkit.Mvvm

# QR Code Scanning
dotnet add package ZXing.Net.Maui
dotnet add package ZXing.Net.Maui.Controls

# Optional but recommended
dotnet add package CommunityToolkit.Maui
```

### 3. Configure MauiProgram.cs

```csharp
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;

namespace QrReaderApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()  // ZXing.Net.Maui
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
        builder.Services.AddSingleton<IPermissionService, PermissionService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<QrScannerViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<QrScannerPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

### 4. Example ViewModel with CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace QrReaderApp.ViewModels;

public partial class QrScannerViewModel : ObservableObject
{
    private readonly IQrCodeService _qrCodeService;
    
    [ObservableProperty]
    private string scannedText = string.Empty;
    
    [ObservableProperty]
    private bool isScanning = true;
    
    [ObservableProperty]
    private ObservableCollection<string> scanHistory = new();

    public QrScannerViewModel(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    [RelayCommand]
    private async Task StartScanningAsync()
    {
        var hasPermission = await CheckCameraPermissionAsync();
        if (!hasPermission) return;
        
        IsScanning = true;
    }

    [RelayCommand]
    private void StopScanning()
    {
        IsScanning = false;
    }

    [RelayCommand]
    private void OnQrCodeDetected(string result)
    {
        ScannedText = result;
        ScanHistory.Insert(0, result);
        
        // Process the QR code
        _qrCodeService.ProcessQrCode(result);
    }

    private async Task<bool> CheckCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        
        return status == PermissionStatus.Granted;
    }
}
```

### 5. Example XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:zxing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.Maui.Controls"
             xmlns:vm="clr-namespace:QrReaderApp.ViewModels"
             x:Class="QrReaderApp.Views.QrScannerPage"
             x:DataType="vm:QrScannerViewModel"
             Title="QR Scanner">
    
    <Grid RowDefinitions="*, Auto, Auto">
        
        <!-- Camera View -->
        <zxing:CameraBarcodeReaderView 
            Grid.Row="0"
            IsDetecting="{Binding IsScanning}"
            BarcodesDetected="OnBarcodesDetected" />
        
        <!-- Scanned Text Display -->
        <Frame Grid.Row="1" 
               BackgroundColor="#80000000" 
               Padding="20"
               Margin="20">
            <Label Text="{Binding ScannedText}" 
                   TextColor="White"
                   FontSize="16"
                   HorizontalOptions="Center"/>
        </Frame>
        
        <!-- Control Buttons -->
        <HorizontalStackLayout Grid.Row="2" 
                              HorizontalOptions="Center"
                              Spacing="20"
                              Margin="20">
            <Button Text="Start Scanning" 
                    Command="{Binding StartScanningCommand}"
                    IsVisible="{Binding IsScanning, Converter={StaticResource InvertedBoolConverter}}"/>
            <Button Text="Stop Scanning" 
                    Command="{Binding StopScanningCommand}"
                    IsVisible="{Binding IsScanning}"/>
        </HorizontalStackLayout>
        
    </Grid>
</ContentPage>
```

### 6. Platform-Specific Permissions

#### Android (Platforms/Android/AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-feature android:name="android.hardware.camera" android:required="false" />
<uses-feature android:name="android.hardware.camera.autofocus" android:required="false" />
```

#### iOS (Platforms/iOS/Info.plist)
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs access to the camera to scan QR codes.</string>
```

## Navigation Strategies

### Option 1: Shell Navigation (Recommended for simple apps)
```csharp
// In AppShell.xaml.cs
Routing.RegisterRoute(nameof(QrScannerPage), typeof(QrScannerPage));

// Navigate
await Shell.Current.GoToAsync(nameof(QrScannerPage));
```

### Option 2: Navigation Service (Better for complex apps)
```csharp
public interface INavigationService
{
    Task NavigateToAsync<TViewModel>() where TViewModel : class;
    Task NavigateBackAsync();
}
```

## Dependency Injection Best Practices

### Service Lifetimes
- **Singleton**: Services that maintain state across the app (e.g., Settings, Cache)
- **Transient**: ViewModels and pages (new instance each time)
- **Scoped**: Not commonly used in MAUI (no request scope like web apps)

### Example Service Registration
```csharp
// Singletons - shared state
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Transient - new instances
builder.Services.AddTransient<MainViewModel>();
builder.Services.AddTransient<QrScannerViewModel>();
builder.Services.AddTransient<MainPage>();
builder.Services.AddTransient<QrScannerPage>();
```

## Testing Considerations

### Unit Testing ViewModels
```csharp
[Fact]
public async Task StartScanning_WithPermission_SetsIsScanningToTrue()
{
    // Arrange
    var mockQrService = new Mock<IQrCodeService>();
    var viewModel = new QrScannerViewModel(mockQrService.Object);
    
    // Act
    await viewModel.StartScanningCommand.ExecuteAsync(null);
    
    // Assert
    Assert.True(viewModel.IsScanning);
}
```

## Performance Tips

1. **Use compiled bindings** with `x:DataType` for better performance
2. **Virtualize lists** with `CollectionView` instead of `ListView`
3. **Dispose camera resources** when page disappears
4. **Use async/await** properly to avoid blocking UI thread
5. **Leverage source generators** from CommunityToolkit.Mvvm

## Common Pitfalls to Avoid

1. **Don't use `Device.BeginInvokeOnMainThread`** - use `MainThread.BeginInvokeOnMainThread` instead
2. **Don't forget platform permissions** - camera access requires explicit permissions
3. **Handle lifecycle events** - pause/resume scanning based on page lifecycle
4. **Test on real devices** - camera features don't work well in emulators

## Quick Start Commands

```bash
# Create project
dotnet new maui -n QrReaderApp
cd QrReaderApp

# Add packages
dotnet add package CommunityToolkit.Mvvm
dotnet add package ZXing.Net.Maui
dotnet add package ZXing.Net.Maui.Controls
dotnet add package CommunityToolkit.Maui

# Build and run
dotnet build
dotnet run
```

## Resources

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [ZXing.Net.Maui GitHub](https://github.com/Redth/ZXing.Net.Maui)
- [MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)

## Next Steps

1. Create the basic project structure
2. Set up DI container in MauiProgram.cs
3. Implement QR scanner service
4. Create ViewModels with CommunityToolkit.Mvvm
5. Build XAML views with data binding
6. Test on physical devices
7. Add error handling and user feedback
8. Implement additional features (history, export, etc.)

---

**Migration Tip**: If you have existing Xamarin Forms code, Microsoft provides a [.NET Upgrade Assistant](https://dotnet.microsoft.com/platform/upgrade-assistant) to help migrate to MAUI.
