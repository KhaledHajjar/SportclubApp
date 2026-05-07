using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/members")]
public sealed class MembersController(
    AppDbContext db,
    IPhotoStorageService photoStorage,
    IValidator<UpdateMemberRequest> updateValidator) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MemberDto>> GetMe(CancellationToken ct)
    {
        var memberId = User.GetMemberId();
        var member = await db.Users.SingleOrDefaultAsync(m => m.Id == memberId, ct);
        if (member is null)
        {
            return NotFound();
        }

        return Ok(ToDto(member.Id, member.Email!, member.FirstName, member.LastName, member.DateOfBirth, member.ProfilePhotoPath));
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberDto>> UpdateMe(UpdateMemberRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var memberId = User.GetMemberId();
        var member = await db.Users.SingleOrDefaultAsync(m => m.Id == memberId, ct);
        if (member is null)
        {
            return NotFound();
        }

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.DateOfBirth = request.DateOfBirth;
        await db.SaveChangesAsync(ct);

        return Ok(ToDto(member.Id, member.Email!, member.FirstName, member.LastName, member.DateOfBirth, member.ProfilePhotoPath));
    }

    [HttpPost("me/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberDto>> UploadPhoto(IFormFile file, CancellationToken ct)
    {
        if (file is null)
        {
            return Problem(detail: "No file uploaded.", statusCode: StatusCodes.Status400BadRequest);
        }

        var memberId = User.GetMemberId();
        var member = await db.Users.SingleOrDefaultAsync(m => m.Id == memberId, ct);
        if (member is null)
        {
            return NotFound();
        }

        try
        {
            member.ProfilePhotoPath = await photoStorage.SaveProfilePhotoAsync(memberId, file, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Invalid photo");
        }

        return Ok(ToDto(member.Id, member.Email!, member.FirstName, member.LastName, member.DateOfBirth, member.ProfilePhotoPath));
    }

    private static MemberDto ToDto(Guid id, string email, string firstName, string lastName, DateOnly? dob, string? photoPath)
        => new(id, email, firstName, lastName, dob, photoPath);
}
