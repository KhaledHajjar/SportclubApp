using Microsoft.AspNetCore.Http;

namespace SportclubApp.Api.Services;

public interface IPhotoStorageService
{
    Task<string> SaveProfilePhotoAsync(Guid memberId, IFormFile file, CancellationToken ct);
}
