using System.Net;

namespace SportclubApp.Maui.Services.Api;

public sealed class ApiException(HttpStatusCode statusCode, string? errorType, string? detail, string? title)
    : Exception(detail ?? title ?? $"API call failed with status {statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string? ErrorType { get; } = errorType;

    public string? Detail { get; } = detail;

    public string? Title { get; } = title;
}

public sealed record ProblemDetailsResponse(string? Type, string? Title, string? Detail, int? Status);
