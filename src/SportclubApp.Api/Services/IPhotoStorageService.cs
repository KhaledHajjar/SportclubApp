using Microsoft.AspNetCore.Http;

namespace SportclubApp.Api.Services;

public sealed record PhotoFile(Stream Stream, string ContentType);

public interface IPhotoStorageService
{
    Task<string> SaveProfilePhotoAsync(Guid memberId, IFormFile file, CancellationToken ct);

    Task<PhotoFile?> OpenProfilePhotoAsync(Guid memberId, CancellationToken ct);
}
