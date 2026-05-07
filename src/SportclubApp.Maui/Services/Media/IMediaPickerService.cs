namespace SportclubApp.Maui.Services.Media;

public interface IMediaPickerService
{
    Task<PhotoPick?> PickAsync(PhotoSource source, CancellationToken ct = default);
}

public enum PhotoSource
{
    Camera,
    Gallery,
}

public sealed record PhotoPick(Stream Content, string FileName, string ContentType) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
