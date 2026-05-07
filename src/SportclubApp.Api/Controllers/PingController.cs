using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Route("api/v1/ping")]
public sealed class PingController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok(new { status = "ok", authenticated = false });

    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult Authenticated() =>
        Ok(new
        {
            status = "ok",
            authenticated = true,
            user = User.Identity?.Name,
            roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value),
        });
}
