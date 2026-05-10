using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.Services.Auth;

public sealed class AuthDelegatingHandler(ISecureTokenStore store) : DelegatingHandler
{
    private const string RefreshPath = "/api/v1/auth/refresh";
    private const string LoginPath = "/api/v1/auth/login";
    private const string RegisterPath = "/api/v1/auth/register";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // PoC limitation: concurrent 401s both call TryRefreshAsync; the second
        // sees a single-use token already consumed and the user gets signed out.
        // Production fix: SemaphoreSlim(1, 1) around the refresh. See README.
        if (IsAnonymous(request))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await store.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync(cancellationToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        if (!await TryRefreshAsync(request.RequestUri!, cancellationToken))
        {
            return response;
        }

        response.Dispose();

        var retry = await CloneRequestAsync(request, cancellationToken);
        var newToken = await store.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(newToken))
        {
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        }
        return await base.SendAsync(retry, cancellationToken);
    }

    private static bool IsAnonymous(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath;
        return path is not null && (path.EndsWith(LoginPath, StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(RegisterPath, StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(RefreshPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> TryRefreshAsync(Uri originalUri, CancellationToken ct)
    {
        var refreshToken = await store.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        var refreshUri = new Uri(originalUri, RefreshPath);
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, refreshUri)
        {
            Content = JsonContent.Create(new RefreshRequest(refreshToken)),
        };

        using var refreshResponse = await base.SendAsync(refreshRequest, ct);
        if (!refreshResponse.IsSuccessStatusCode)
        {
            await store.ClearAsync();
            return false;
        }

        var payload = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(ct);
        if (payload is null)
        {
            await store.ClearAsync();
            return false;
        }

        await store.SaveTokensAsync(payload.AccessToken, payload.RefreshToken);
        return true;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage source, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri)
        {
            Version = source.Version,
            VersionPolicy = source.VersionPolicy,
        };

        foreach (var header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (source.Content is not null)
        {
            await source.Content.LoadIntoBufferAsync(ct);
            var bytes = await source.Content.ReadAsByteArrayAsync(ct);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in source.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
