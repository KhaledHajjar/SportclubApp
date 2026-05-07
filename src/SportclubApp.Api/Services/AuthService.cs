using Microsoft.AspNetCore.Identity;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Api.Services;

public sealed class AuthService(
    UserManager<Member> userManager,
    SignInManager<Member> signInManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService) : IAuthService
{
    public async Task<AuthOutcome> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return AuthOutcome.Fail("A member with this email already exists.");
        }

        var member = new Member
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true,
        };

        var create = await userManager.CreateAsync(member, request.Password);
        if (!create.Succeeded)
        {
            return AuthOutcome.Fail(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(member, AuthRoles.Member);

        return await IssueTokensAsync(member, ct);
    }

    public async Task<AuthOutcome> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var member = await userManager.FindByEmailAsync(request.Email);
        if (member is null)
        {
            return AuthOutcome.Fail("Invalid email or password.");
        }

        var check = await signInManager.CheckPasswordSignInAsync(member, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
        {
            return AuthOutcome.Fail("Invalid email or password.");
        }

        return await IssueTokensAsync(member, ct);
    }

    public async Task<AuthOutcome> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var rotated = await refreshTokenService.RotateAsync(refreshToken, ct);
        if (rotated is null)
        {
            return AuthOutcome.Fail("Refresh token is invalid, expired, or revoked.");
        }

        var member = rotated.Member;
        var roles = await userManager.GetRolesAsync(member);
        var access = jwtTokenService.Create(member, roles);

        return AuthOutcome.Ok(new AuthResponse(
            MemberId: member.Id,
            Email: member.Email ?? string.Empty,
            AccessToken: access.Value,
            AccessTokenExpiresUtc: access.ExpiresUtc,
            RefreshToken: rotated.Token,
            RefreshTokenExpiresUtc: rotated.ExpiresUtc,
            Roles: [.. roles]));
    }

    public Task<bool> LogoutAsync(string refreshToken, CancellationToken ct) =>
        refreshTokenService.RevokeAsync(refreshToken, ct);

    private async Task<AuthOutcome> IssueTokensAsync(Member member, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(member);
        var access = jwtTokenService.Create(member, roles);
        var refresh = await refreshTokenService.IssueAsync(member, ct);

        return AuthOutcome.Ok(new AuthResponse(
            MemberId: member.Id,
            Email: member.Email ?? string.Empty,
            AccessToken: access.Value,
            AccessTokenExpiresUtc: access.ExpiresUtc,
            RefreshToken: refresh.Token,
            RefreshTokenExpiresUtc: refresh.ExpiresUtc,
            Roles: [.. roles]));
    }
}
