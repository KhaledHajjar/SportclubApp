using System.Security.Claims;

namespace SportclubApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetMemberId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Authenticated principal has no member id claim.");

        return Guid.Parse(raw);
    }
}
