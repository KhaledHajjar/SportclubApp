using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Dtos.Admin;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize(Roles = AuthRoles.Admin)]
[Route("api/v1/admin")]
public sealed class AdminController(IAdminService admin) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> Stats(CancellationToken ct)
        => Ok(await admin.GetStatsAsync(ct));

    [HttpGet("members")]
    [ProducesResponseType(typeof(IReadOnlyList<MemberAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MemberAdminDto>>> Members(
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await admin.GetMembersAsync(search, ct));

    [HttpGet("plans")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanAdminDto>>> Plans(CancellationToken ct)
        => Ok(await admin.GetPlansAsync(ct));

    [HttpGet("class-sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClassSessionDto>>> ClassSessions(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var rangeFrom = from ?? DateTimeOffset.UtcNow;
        var rangeTo = to ?? rangeFrom.AddDays(7);
        return Ok(await admin.GetClassSessionsAsync(rangeFrom, rangeTo, ct));
    }

    [HttpGet("reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationAdminDto>>> Reservations(
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
        => Ok(await admin.GetReservationsAsync(Math.Clamp(limit, 1, 200), ct));
}
