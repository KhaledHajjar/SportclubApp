using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/classes")]
public sealed class ClassSessionsController(IClassSessionService classSessions) : ControllerBase
{
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(7);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClassSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ClassSessionDto>>> GetSchedule(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        var toUtc = to?.ToUniversalTime() ?? fromUtc + MaxRange;

        if (toUtc <= fromUtc)
        {
            return Problem(detail: "'to' must be after 'from'.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (toUtc - fromUtc > MaxRange)
        {
            toUtc = fromUtc + MaxRange;
        }

        var schedule = await classSessions.GetScheduleAsync(fromUtc, toUtc, ct);
        return Ok(schedule);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClassSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassSessionDto>> GetById(Guid id, CancellationToken ct)
    {
        var session = await classSessions.GetByIdAsync(id, ct);
        return session is null ? NotFound() : Ok(session);
    }
}
