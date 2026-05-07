using FluentValidation;
using Scalar.AspNetCore;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSportclubIdentity();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddOpenApiWithBearer();

builder.Services.AddScoped<IPhotoStorageService, PhotoStorageService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

var app = builder.Build();

await app.Services.EnsureRolesCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sportclub API")
               .WithTheme(ScalarTheme.BluePlanet);
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
