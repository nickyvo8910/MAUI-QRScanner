namespace QrReaderApp.Services;

public interface IPermissionService
{
    Task<bool> CheckAndRequestCameraPermissionAsync();
}
