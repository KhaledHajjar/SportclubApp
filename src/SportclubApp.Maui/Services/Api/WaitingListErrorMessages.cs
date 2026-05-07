using SportclubApp.Shared.Errors;

namespace SportclubApp.Maui.Services.Api;

public static class WaitingListErrorMessages
{
    public static string ForJoin(ApiException ex) => ex.ErrorType switch
    {
        ReservationErrorTypes.ClassNotFound => "This class no longer exists.",
        ReservationErrorTypes.ClassAlreadyStarted => "This class has already started.",
        ReservationErrorTypes.AlreadyReserved => "You already have a reservation for this class.",
        WaitingListErrorTypes.ClassNotFull => "This class still has free spots; reserve directly instead.",
        WaitingListErrorTypes.AlreadyOnWaitingList => "You're already on the waiting list.",
        _ => ex.Detail ?? "Could not join the waiting list.",
    };

    public static string ForLeave(ApiException ex) => ex.ErrorType switch
    {
        WaitingListErrorTypes.WaitingListEntryNotFound => "Your waiting-list entry was already removed.",
        WaitingListErrorTypes.WaitingListEntryNotOwned => "You can't remove someone else's waiting-list entry.",
        _ => ex.Detail ?? "Could not leave the waiting list.",
    };
}
