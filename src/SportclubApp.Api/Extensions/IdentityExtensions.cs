using Microsoft.AspNetCore.Identity;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Api.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddSportclubIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<Member>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    public static async Task EnsureRolesCreatedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in new[] { AuthRoles.Member, AuthRoles.Instructor, AuthRoles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
            }
        }
    }
}
