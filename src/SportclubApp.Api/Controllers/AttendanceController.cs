using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/attendance")]
public sealed class AttendanceController(IAttendanceService attendance) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<AttendanceRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetMine(
        [FromQuery] int? year,
        CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var resolvedYear = year ?? DateTime.UtcNow.Year;
        return Ok(await attendance.GetHistoryAsync(memberId, resolvedYear, ct));
    }
}
