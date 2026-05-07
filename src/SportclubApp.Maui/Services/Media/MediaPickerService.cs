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

        FileResult? result;
        if (source == PhotoSource.Camera)
        {
            result = await MediaPicker.Default.CapturePhotoAsync();
        }
        else
        {
            var picked = await MediaPicker.Default.PickPhotosAsync();
            result = picked?.FirstOrDefault();
        }

        if (result is null)
        {
            return null;
        }

        var stream = await result.OpenReadAsync();
        var contentType = string.IsNullOrEmpty(result.ContentType) ? "image/jpeg" : result.ContentType;
        return new PhotoPick(stream, result.FileName ?? "photo.jpg", contentType);
    }
}
