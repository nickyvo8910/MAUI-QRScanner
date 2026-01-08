namespace QrReaderApp.Services;

public class PermissionService : IPermissionService
{
    public async Task<bool> CheckAndRequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // On iOS, once denied, user must go to settings
            return false;
        }

        status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }
}
