using Microsoft.AspNetCore.Identity;

namespace SportclubApp.Api.Entities;

public sealed class Member : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<WaitingListEntry> WaitingListEntries { get; set; } = [];
    public ICollection<AttendanceRecord> Attendance { get; set; } = [];
    public ICollection<DeviceToken> DeviceTokens { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
