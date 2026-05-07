namespace SportclubApp.Shared.Push;

public static class PushNotificationTypes
{
    public const string SlotOpened = "slot-opened";
    public const string SubscriptionExpiry = "subscription-expiry-warning";
}

public static class PushPayloadKeys
{
    public const string Type = "type";
    public const string ClassSessionId = "classSessionId";
    public const string SubscriptionId = "subscriptionId";
    public const string ReservationId = "reservationId";
}
