using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Auth;
using SportclubApp.Maui.Services.Media;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.ViewModels;
using SportclubApp.Maui.Views;

namespace SportclubApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<ISecureTokenStore, SecureTokenStore>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IMediaPickerService, MediaPickerService>();
        builder.Services.AddTransient<AuthDelegatingHandler>();

        builder.Services
            .AddHttpClient<ISportclubApi, SportclubApi>(client =>
            {
                client.BaseAddress = new Uri(AppConstants.ApiBaseUrl);
            })
            .AddHttpMessageHandler<AuthDelegatingHandler>()
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            })
#endif
            ;

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ScheduleViewModel>();
        builder.Services.AddTransient<SchedulePage>();
        builder.Services.AddTransient<ClassDetailViewModel>();
        builder.Services.AddTransient<ClassDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
