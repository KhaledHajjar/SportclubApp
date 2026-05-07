using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Member, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Member>(b =>
        {
            b.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
            b.Property(m => m.LastName).IsRequired().HasMaxLength(100);
            b.Property(m => m.ProfilePhotoPath).HasMaxLength(500);
        });

        builder.Entity<Instructor>(b =>
        {
            b.Property(i => i.FirstName).IsRequired().HasMaxLength(100);
            b.Property(i => i.LastName).IsRequired().HasMaxLength(100);
            b.Property(i => i.Bio).HasMaxLength(1000);
            b.HasOne(i => i.Member)
             .WithMany()
             .HasForeignKey(i => i.MemberId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Workout>(b =>
        {
            b.Property(w => w.Name).IsRequired().HasMaxLength(150);
            b.Property(w => w.Description).IsRequired().HasMaxLength(2000);
        });

        builder.Entity<Location>(b =>
        {
            b.Property(l => l.Name).IsRequired().HasMaxLength(150);
            b.Property(l => l.Address).HasMaxLength(300);
        });

        builder.Entity<ClassSession>(b =>
        {
            b.Property(c => c.Capacity).IsRequired();
            b.HasIndex(c => c.StartUtc);
            b.HasOne(c => c.Workout).WithMany(w => w.ClassSessions).HasForeignKey(c => c.WorkoutId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.Instructor).WithMany(i => i.ClassSessions).HasForeignKey(c => c.InstructorId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.Location).WithMany(l => l.ClassSessions).HasForeignKey(c => c.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Reservation>(b =>
        {
            b.Property(r => r.Status).HasConversion<int>();
            b.HasOne(r => r.Member).WithMany(m => m.Reservations).HasForeignKey(r => r.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.ClassSession).WithMany(c => c.Reservations).HasForeignKey(r => r.ClassSessionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(r => new { r.MemberId, r.ClassSessionId, r.Status });
        });

        builder.Entity<WaitingListEntry>(b =>
        {
            b.HasOne(w => w.Member).WithMany(m => m.WaitingListEntries).HasForeignKey(w => w.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(w => w.ClassSession).WithMany(c => c.WaitingList).HasForeignKey(w => w.ClassSessionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(w => new { w.MemberId, w.ClassSessionId }).IsUnique();
            b.HasIndex(w => new { w.ClassSessionId, w.Position });
        });

        builder.Entity<Subscription>(b =>
        {
            b.Property(s => s.Type).HasConversion<int>();
            b.HasOne(s => s.Member).WithMany(m => m.Subscriptions).HasForeignKey(s => s.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(s => new { s.MemberId, s.EndUtc });
        });

        builder.Entity<AttendanceRecord>(b =>
        {
            b.HasOne(a => a.Member).WithMany(m => m.Attendance).HasForeignKey(a => a.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.ClassSession).WithMany().HasForeignKey(a => a.ClassSessionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(a => new { a.MemberId, a.AttendedUtc });
        });

        builder.Entity<DeviceToken>(b =>
        {
            b.Property(d => d.Token).IsRequired().HasMaxLength(500);
            b.Property(d => d.Platform).HasConversion<int>();
            b.HasOne(d => d.Member).WithMany(m => m.DeviceTokens).HasForeignKey(d => d.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(d => d.Token).IsUnique();
        });
    }
}
