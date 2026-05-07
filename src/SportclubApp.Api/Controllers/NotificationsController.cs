using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMine(
        [FromQuery] bool includeRead = false,
        CancellationToken ct = default)
    {
        var memberId = User.GetMemberId();
        return Ok(await notifications.GetMineAsync(memberId, includeRead, ct));
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnreadCountDto>> UnreadCount(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var count = await notifications.GetUnreadCountAsync(memberId, ct);
        return Ok(new UnreadCountDto(count));
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var ok = await notifications.MarkAsReadAsync(memberId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnreadCountDto>> MarkAllRead(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var marked = await notifications.MarkAllAsReadAsync(memberId, ct);
        return Ok(new UnreadCountDto(marked));
    }
}
