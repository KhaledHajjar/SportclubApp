using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;

namespace SportclubApp.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }
}
