namespace SportclubApp.Shared.Dtos.Admin;

public sealed record AdminStatsDto(
    int TotalMembers,
    int ActiveSubscriptions,
    int ClassesThisWeek,
    int ReservationsThisWeek);
