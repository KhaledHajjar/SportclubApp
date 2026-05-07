namespace SportclubApp.Maui;

public static class AppConstants
{
    public static string ApiBaseUrl =>
#if ANDROID
        "https://10.0.2.2:5001";
#else
        "https://localhost:5001";
#endif
}
