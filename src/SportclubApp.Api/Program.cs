using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SportclubApp.Api.Common.Events;
using SportclubApp.Api.Common.Events.Handlers;
using SportclubApp.Api.Data;
using SportclubApp.Api.Extensions;
using SportclubApp.Api.Services;
using SportclubApp.Api.Services.Policies;
using SportclubApp.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSportclubIdentity();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddOpenApiWithBearer();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPlanCancellationPolicyFactory, PlanCancellationPolicyFactory>();
builder.Services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<SlotOpenedEvent>, SlotOpenedEventLoggingHandler>();
builder.Services.AddScoped<IDomainEventHandler<SlotOpenedEvent>, SlotOpenedNotificationHandler>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPhotoStorageService, PhotoStorageService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IClassSessionService, ClassSessionService>();
builder.Services.AddScoped<IWaitingListService, WaitingListService>();
builder.Services.AddScoped<IWaitingListPromotionService, WaitingListPromotionService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await app.Services.EnsureRolesCreatedAsync();

if (args.Length > 0 && string.Equals(args[0], "seed", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
    return;
}

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
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
