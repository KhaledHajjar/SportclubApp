using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;

namespace SportclubApp.Api.Services;

public sealed class RefreshTokenService(AppDbContext db, IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<RefreshToken> IssueAsync(Member member, CancellationToken ct)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Token = GenerateToken(),
            CreatedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
        };

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<RefreshToken?> RotateAsync(string token, CancellationToken ct)
    {
        var existing = await db.RefreshTokens
            .Include(r => r.Member)
            .SingleOrDefaultAsync(r => r.Token == token, ct);

        if (existing is null || !existing.IsActive)
        {
            return null;
        }

        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            MemberId = existing.MemberId,
            Member = existing.Member,
            Token = GenerateToken(),
            CreatedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
        };

        existing.RevokedUtc = DateTimeOffset.UtcNow;
        existing.ReplacedByToken = replacement.Token;

        db.RefreshTokens.Add(replacement);
        await db.SaveChangesAsync(ct);
        return replacement;
    }

    public async Task<bool> RevokeAsync(string token, CancellationToken ct)
    {
        var existing = await db.RefreshTokens.SingleOrDefaultAsync(r => r.Token == token, ct);
        if (existing is null || existing.IsRevoked)
        {
            return false;
        }

        existing.RevokedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
