namespace SportclubApp.Api.Services.Push;

public sealed class FcmOptions
{
    public const string SectionName = "Fcm";

    public string? ServiceAccountJsonPath { get; init; }
}
