using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Api.Extensions;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me/devices")]
public sealed class DevicesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Register(RegisterDeviceRequest request, CancellationToken ct)
    {
        var memberId = User.GetMemberId();

        var existing = await db.DeviceTokens.SingleOrDefaultAsync(d => d.Token == request.Token, ct);
        if (existing is null)
        {
            db.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                Token = request.Token,
                Platform = request.Platform,
                RegisteredUtc = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            existing.MemberId = memberId;
            existing.Platform = request.Platform;
            existing.RegisteredUtc = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unregister(string token, CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var device = await db.DeviceTokens.SingleOrDefaultAsync(d => d.Token == token && d.MemberId == memberId, ct);
        if (device is not null)
        {
            db.DeviceTokens.Remove(device);
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }
}
