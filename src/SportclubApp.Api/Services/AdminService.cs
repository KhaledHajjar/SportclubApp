using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Dtos.Admin;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services;

public sealed class AdminService(AppDbContext db, IClassSessionService classSessions) : IAdminService
{
    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);
        var nextWeek = now.AddDays(7);

        // "Members" = users who have the Member role specifically
        var memberRoleId = await db.Roles
            .Where(r => r.Name == AuthRoles.Member)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);
        var totalMembers = await db.UserRoles
            .CountAsync(ur => ur.RoleId == memberRoleId, ct);

        var activeSubscriptions = await db.Subscriptions
            .CountAsync(s => s.StartUtc <= now && s.EndUtc > now, ct);

        var classesThisWeek = await db.ClassSessions
            .CountAsync(c => c.StartUtc >= now && c.StartUtc < nextWeek, ct);

        var reservationsThisWeek = await db.Reservations
            .CountAsync(r => r.CreatedUtc >= weekStart && r.CreatedUtc <= now, ct);

        return new AdminStatsDto(totalMembers, activeSubscriptions, classesThisWeek, reservationsThisWeek);
    }

    public async Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(string? search, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        // Roles by user (single round-trip)
        var roleNames = await db.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name ?? string.Empty, ct);
        var userRoleRows = await db.UserRoles.AsNoTracking().ToListAsync(ct);
        var rolesByUser = userRoleRows
            .GroupBy(ur => ur.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(ur => roleNames[ur.RoleId]).ToList());

        // Active subscription per member (latest if multiple)
        var activeSubs = await db.Subscriptions
            .AsNoTracking()
            .Where(s => s.StartUtc <= now && s.EndUtc > now)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.EndUtc)
            .ToListAsync(ct);
        var subByMember = activeSubs
            .GroupBy(s => s.MemberId)
            .ToDictionary(g => g.Key, g => g.First());

        // Member list with optional search
        var query = db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(m =>
                EF.Functions.Like(m.FirstName, pattern)
                || EF.Functions.Like(m.LastName, pattern)
                || (m.Email != null && EF.Functions.Like(m.Email, pattern)));
        }
        var members = await query.OrderBy(m => m.FirstName).ToListAsync(ct);

        return members.Select(m =>
        {
            var memberRoles = rolesByUser.GetValueOrDefault(m.Id, Array.Empty<string>());
            subByMember.TryGetValue(m.Id, out var sub);
            return new MemberAdminDto(
                Id: m.Id,
                Email: m.Email ?? string.Empty,
                FirstName: m.FirstName,
                LastName: m.LastName,
                DateOfBirth: m.DateOfBirth,
                Roles: memberRoles,
                ActivePlanName: sub?.Plan.Name,
                PlanEndsUtc: sub?.EndUtc,
                CreatedUtc: m.CreatedUtc);
        }).ToList();
    }

    public async Task<IReadOnlyList<PlanAdminDto>> GetPlansAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var plans = await db.Plans
            .AsNoTracking()
            .OrderBy(p => p.Tier)
            .ThenBy(p => p.BillingPeriod)
            .ToListAsync(ct);

        var activeCounts = await db.Subscriptions
            .Where(s => s.StartUtc <= now && s.EndUtc > now)
            .GroupBy(s => s.PlanId)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId, x => x.Count, ct);

        return plans.Select(p => new PlanAdminDto(
            p.Id,
            p.Name,
            p.Tier,
            p.BillingPeriod,
            p.DurationDays,
            p.Price,
            activeCounts.GetValueOrDefault(p.Id, 0))).ToList();
    }

    public Task<IReadOnlyList<ClassSessionDto>> GetClassSessionsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
        => classSessions.GetScheduleAsync(from, to, ct);

    public async Task<IReadOnlyList<ReservationAdminDto>> GetReservationsAsync(int limit, CancellationToken ct)
    {
        return await db.Reservations
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedUtc)
            .Take(limit)
            .Select(r => new ReservationAdminDto(
                r.Id,
                r.MemberId,
                r.Member.FirstName + " " + r.Member.LastName,
                r.ClassSessionId,
                r.ClassSession.Workout.Name,
                r.ClassSession.StartUtc,
                r.CreatedUtc,
                r.Status))
            .ToListAsync(ct);
    }
}
