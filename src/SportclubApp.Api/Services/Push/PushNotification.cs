using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Push;

public sealed record PushNotification(
    string Title,
    string Body,
    IReadOnlyDictionary<string, string> Data);

public sealed record DeviceTarget(string Token, DevicePlatform Platform);
