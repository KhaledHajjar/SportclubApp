namespace SportclubApp.Maui.Services.Auth;

public sealed class SecureTokenStore : ISecureTokenStore
{
    private const string AccessKey = "sportclub.access-token";
    private const string RefreshKey = "sportclub.refresh-token";

    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessKey);

    public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshKey);

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await SecureStorage.Default.SetAsync(AccessKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessKey);
        SecureStorage.Default.Remove(RefreshKey);
        return Task.CompletedTask;
    }
}
