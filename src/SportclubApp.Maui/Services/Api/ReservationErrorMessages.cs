using SportclubApp.Shared.Errors;

namespace SportclubApp.Maui.Services.Api;

public static class ReservationErrorMessages
{
    public static string ForReserve(ApiException ex) => ex.ErrorType switch
    {
        ReservationErrorTypes.ClassNotFound => "This class no longer exists.",
        ReservationErrorTypes.ClassAlreadyStarted => "This class has already started.",
        ReservationErrorTypes.ClassTooFarAhead => "Reservations open at most one week ahead.",
        ReservationErrorTypes.ClassFull => "This class is full. Join the waiting list instead.",
        ReservationErrorTypes.NoActiveSubscription => "You don't have an active subscription.",
        ReservationErrorTypes.WeeklyLimitReached => "You've already used both visits for this week.",
        ReservationErrorTypes.AlreadyReserved => "You already have a reservation for this class.",
        _ => ex.Detail ?? "Could not reserve. Please try again.",
    };

    public static string ForCancel(ApiException ex) => ex.ErrorType switch
    {
        ReservationErrorTypes.ReservationNotFound => "This reservation no longer exists.",
        ReservationErrorTypes.ReservationNotOwned => "You can't cancel someone else's reservation.",
        ReservationErrorTypes.CancelTooLate => "You can only cancel up to one hour before class start.",
        _ => ex.Detail ?? "Could not cancel. Please try again.",
    };
}
