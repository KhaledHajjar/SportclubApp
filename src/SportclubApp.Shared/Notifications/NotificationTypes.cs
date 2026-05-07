namespace SportclubApp.Shared.Notifications;

public static class NotificationTypes
{
    public const string SlotOpened = "slot-opened";
    public const string SubscriptionExpiry = "subscription-expiry-warning";
}

public static class NotificationDataKeys
{
    public const string ClassSessionId = "classSessionId";
    public const string ReservationId = "reservationId";
    public const string SubscriptionId = "subscriptionId";
}
