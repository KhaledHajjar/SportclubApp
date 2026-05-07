using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthService auth,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        if (await ApplyValidationAsync(request, registerValidator, ct) is { } problem)
        {
            return problem;
        }

        var outcome = await auth.RegisterAsync(request, ct);
        return outcome.Success
            ? Ok(outcome.Response)
            : Problem(detail: outcome.Error, statusCode: StatusCodes.Status409Conflict, title: "Registration failed");
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (await ApplyValidationAsync(request, loginValidator, ct) is { } problem)
        {
            return problem;
        }

        var outcome = await auth.LoginAsync(request, ct);
        return outcome.Success
            ? Ok(outcome.Response)
            : Problem(detail: outcome.Error, statusCode: StatusCodes.Status401Unauthorized, title: "Login failed");
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var outcome = await auth.RefreshAsync(request.RefreshToken, ct);
        return outcome.Success
            ? Ok(outcome.Response)
            : Problem(detail: outcome.Error, statusCode: StatusCodes.Status401Unauthorized, title: "Refresh failed");
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    private async Task<ActionResult?> ApplyValidationAsync<T>(T request, IValidator<T> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid)
        {
            return null;
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        return ValidationProblem(ModelState);
    }
}
