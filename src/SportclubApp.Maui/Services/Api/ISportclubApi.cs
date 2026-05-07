using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.Services.Api;

public interface ISportclubApi
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);

    Task<MemberDto> GetMeAsync(CancellationToken ct = default);

    Task<MemberDto> UpdateMeAsync(UpdateMemberRequest request, CancellationToken ct = default);

    Task<MemberDto> UploadProfilePhotoAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    Task<SubscriptionDto?> GetMySubscriptionAsync(CancellationToken ct = default);
}
