using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Dtos.Admin;

namespace SportclubApp.Api.Services;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken ct);

    Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(string? search, CancellationToken ct);

    Task<IReadOnlyList<PlanAdminDto>> GetPlansAsync(CancellationToken ct);

    Task<IReadOnlyList<ClassSessionDto>> GetClassSessionsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    Task<IReadOnlyList<ReservationAdminDto>> GetReservationsAsync(int limit, CancellationToken ct);
}
