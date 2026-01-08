namespace QrReaderApp.Services;

public interface IQrCodeService
{
    Task<string> GenerateQrCodeAsync(string content);
    bool ValidateQrCode(string content);
}
