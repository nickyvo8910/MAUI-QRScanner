using ZXing.Net.Maui;

namespace QrReaderApp.Services;

public class QrCodeService : IQrCodeService
{
    public Task<string> GenerateQrCodeAsync(string content)
    {
        // This can be extended to generate QR codes if needed
        return Task.FromResult(content);
    }

    public bool ValidateQrCode(string content)
    {
        // Basic validation - check if content is not empty
        return !string.IsNullOrWhiteSpace(content);
    }
}
