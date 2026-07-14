using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection addInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "La connection string 'DefaultConnection' est introuvable (User Secrets ?).");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;

    }
}