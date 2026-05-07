using SportclubApp.Api.Entities;

namespace SportclubApp.Api.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> IssueAsync(Member member, CancellationToken ct);

    Task<RefreshToken?> RotateAsync(string token, CancellationToken ct);

    Task<bool> RevokeAsync(string token, CancellationToken ct);
}
