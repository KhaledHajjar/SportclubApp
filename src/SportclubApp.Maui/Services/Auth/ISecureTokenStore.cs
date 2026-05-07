namespace SportclubApp.Maui.Services.Auth;

public interface ISecureTokenStore
{
    Task<string?> GetAccessTokenAsync();

    Task<string?> GetRefreshTokenAsync();

    Task SaveTokensAsync(string accessToken, string refreshToken);

    Task ClearAsync();
}
