using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/subscriptions")]
public sealed class SubscriptionsController(ISubscriptionService subscriptions) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> GetMine(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var current = await subscriptions.GetCurrentAsync(memberId, ct);

        return current is null ? NotFound() : Ok(current);
    }
}
