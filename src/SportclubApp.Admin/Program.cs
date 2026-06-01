using SportclubApp.Admin.Components;
using SportclubApp.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Components with interactive server-rendered components.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth state lives per Blazor Server circuit (per signed-in user). Scoped service
// resolved through the circuit's DI scope so the Login page and every other page
// share the same AccessToken / RefreshToken.
builder.Services.AddScoped<AdminAuthState>();

// Typed HttpClient for the admin API. Bearer attachment + 401-refresh-retry are
// handled INSIDE AdminApi rather than via a DelegatingHandler — IHttpClientFactory
// would resolve a delegating handler in its own scope, not the circuit's, and
// the handler would see an empty AdminAuthState.
var apiBaseUrl = builder.Configuration["AdminApiBaseUrl"]
    ?? throw new InvalidOperationException("AdminApiBaseUrl is not configured.");

builder.Services
    .AddHttpClient<IAdminApi, AdminApi>(client => client.BaseAddress = new Uri(apiBaseUrl))
#if DEBUG
    // The ASP.NET Core HTTPS dev cert isn't trusted by every Blazor host
    // out of the box; bypass validation in DEBUG only, never in Release.
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
    })
#endif
    ;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
