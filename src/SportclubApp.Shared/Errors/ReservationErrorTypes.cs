namespace SportclubApp.Shared.Errors;

public static class ReservationErrorTypes
{
    private const string Prefix = "https://sportclub.api/errors/";

    public const string ClassNotFound = Prefix + "class-not-found";
    public const string ClassAlreadyStarted = Prefix + "class-already-started";
    public const string ClassTooFarAhead = Prefix + "class-too-far-ahead";
    public const string ClassFull = Prefix + "class-full";
    public const string NoActiveSubscription = Prefix + "no-active-subscription";
    public const string WeeklyLimitReached = Prefix + "weekly-limit-reached";
    public const string AlreadyReserved = Prefix + "already-reserved";
    public const string ReservationNotFound = Prefix + "reservation-not-found";
    public const string ReservationNotOwned = Prefix + "reservation-not-owned";
    public const string CancelTooLate = Prefix + "cancel-too-late";
}
