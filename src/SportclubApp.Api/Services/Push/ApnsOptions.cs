namespace SportclubApp.Api.Services.Push;

public sealed class ApnsOptions
{
    public const string SectionName = "Apns";

    public string? KeyPath { get; init; }
    public string? KeyId { get; init; }
    public string? TeamId { get; init; }
    public string? BundleId { get; init; }
    public bool UseSandbox { get; init; } = true;
}
