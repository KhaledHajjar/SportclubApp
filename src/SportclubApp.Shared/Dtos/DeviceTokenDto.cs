using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos;

public sealed record RegisterDeviceRequest(
    string Token,
    DevicePlatform Platform);
