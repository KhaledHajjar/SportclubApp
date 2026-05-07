using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/reservations")]
public sealed class ReservationsController(IReservationService reservations) : ControllerBase
{
    [HttpPost("/api/v1/classes/{classId:guid}/reservations")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Reserve(Guid classId, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var result = await reservations.ReserveAsync(memberId, classId, ct);

        if (result.Success)
        {
            return CreatedAtAction(nameof(GetMine), new { }, result.Reservation);
        }

        return ToProblem(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Cancel(Guid id, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var result = await reservations.CancelAsync(memberId, id, ct);

        return result.Success ? Ok(result.Reservation) : ToProblem(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetMine(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        return Ok(await reservations.GetMineAsync(memberId, ct));
    }

    private ActionResult ToProblem(ReservationResult result)
    {
        var status = result.ErrorType switch
        {
            ReservationErrorTypes.ClassNotFound => StatusCodes.Status404NotFound,
            ReservationErrorTypes.ReservationNotFound => StatusCodes.Status404NotFound,
            ReservationErrorTypes.ReservationNotOwned => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status409Conflict,
        };

        return Problem(
            type: result.ErrorType,
            detail: result.ErrorDetail,
            statusCode: status,
            title: "Reservation operation failed");
    }
}
