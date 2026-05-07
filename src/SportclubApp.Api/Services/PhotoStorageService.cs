using Microsoft.AspNetCore.Hosting;

namespace SportclubApp.Api.Services;

public sealed class PhotoStorageService(IWebHostEnvironment env) : IPhotoStorageService
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private const string UploadsFolder = "uploads";

    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/png"] = ".png",
    };

    public async Task<string> SaveProfilePhotoAsync(Guid memberId, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if (file.Length > MaxBytes)
        {
            throw new InvalidOperationException($"File exceeds the {MaxBytes / (1024 * 1024)} MB limit.");
        }

        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var extension))
        {
            throw new InvalidOperationException("Only JPEG and PNG images are accepted.");
        }

        var uploadsRoot = Path.Combine(env.WebRootPath, UploadsFolder);
        Directory.CreateDirectory(uploadsRoot);

        foreach (var existing in Directory.EnumerateFiles(uploadsRoot, $"{memberId}.*"))
        {
            File.Delete(existing);
        }

        var fileName = $"{memberId}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return $"/{UploadsFolder}/{fileName}";
    }
}
