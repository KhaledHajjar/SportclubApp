using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Dtos.Admin;

namespace SportclubApp.Admin.Services;

public interface IAdminApi
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);

    Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(string? search, CancellationToken ct = default);

    Task<IReadOnlyList<PlanAdminDto>> GetPlansAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClassSessionDto>> GetClassSessionsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationAdminDto>> GetReservationsAsync(int limit, CancellationToken ct = default);
}
