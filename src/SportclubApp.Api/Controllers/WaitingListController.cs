using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/waiting-list")]
public sealed class WaitingListController(IWaitingListService waitingList) : ControllerBase
{
    [HttpPost("/api/v1/classes/{classId:guid}/waiting-list")]
    [ProducesResponseType(typeof(WaitingListEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WaitingListEntryDto>> Join(Guid classId, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var result = await waitingList.JoinAsync(memberId, classId, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetMine), new { }, result.Entry)
            : ToProblem(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(WaitingListEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WaitingListEntryDto>> Leave(Guid id, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var result = await waitingList.LeaveAsync(memberId, id, ct);
        return result.Success ? Ok(result.Entry) : ToProblem(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<WaitingListEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WaitingListEntryDto>>> GetMine(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        return Ok(await waitingList.GetMineAsync(memberId, ct));
    }

    private ActionResult ToProblem(WaitingListResult result)
    {
        var status = result.ErrorType switch
        {
            ReservationErrorTypes.ClassNotFound => StatusCodes.Status404NotFound,
            WaitingListErrorTypes.WaitingListEntryNotFound => StatusCodes.Status404NotFound,
            WaitingListErrorTypes.WaitingListEntryNotOwned => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status409Conflict,
        };

        return Problem(
            type: result.ErrorType,
            detail: result.ErrorDetail,
            statusCode: status,
            title: "Waiting list operation failed");
    }
}
