namespace SportclubApp.Maui.Services.Media;

public sealed class MediaPickerService : IMediaPickerService
{
    public async Task<PhotoPick?> PickAsync(PhotoSource source, CancellationToken ct = default)
    {
        var permission = source == PhotoSource.Camera
            ? await Permissions.RequestAsync<Permissions.Camera>()
            : await Permissions.RequestAsync<Permissions.Photos>();

        if (permission != PermissionStatus.Granted)
        {
            return null;
        }

        var result = source == PhotoSource.Camera
            ? await MediaPicker.Default.CapturePhotoAsync()
            : await MediaPicker.Default.PickPhotoAsync();

        if (result is null)
        {
            return null;
        }

        var stream = await result.OpenReadAsync();
        var contentType = string.IsNullOrEmpty(result.ContentType) ? "image/jpeg" : result.ContentType;
        return new PhotoPick(stream, result.FileName ?? "photo.jpg", contentType);
    }
}
