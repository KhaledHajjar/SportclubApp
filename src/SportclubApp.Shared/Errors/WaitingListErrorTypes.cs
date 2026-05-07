namespace SportclubApp.Shared.Errors;

public static class WaitingListErrorTypes
{
    private const string Prefix = "https://sportclub.api/errors/";

    public const string ClassNotFull = Prefix + "class-not-full";
    public const string AlreadyOnWaitingList = Prefix + "already-on-waiting-list";
    public const string WaitingListEntryNotFound = Prefix + "waiting-list-entry-not-found";
    public const string WaitingListEntryNotOwned = Prefix + "waiting-list-entry-not-owned";
}
