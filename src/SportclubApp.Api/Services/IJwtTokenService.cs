using SportclubApp.Api.Entities;

namespace SportclubApp.Api.Services;

public interface IJwtTokenService
{
    AccessToken Create(Member member, IEnumerable<string> roles);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresUtc);
