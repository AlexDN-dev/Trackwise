using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trackwise.Application.Auth;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Entreprises;
using Trackwise.Infrastructure.AI;
using Trackwise.Infrastructure.Authentication;
using Trackwise.Infrastructure.Persistence;
using Trackwise.Infrastructure.Repositories;

namespace Trackwise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "La connection string 'DefaultConnection' est introuvable (User Secrets ?).");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICandidatureRepository, CandidatureRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IEntrepriseRepository, EntrepriseRepository>();

        services.Configure<OllamaSettings>(configuration.GetSection(OllamaSettings.SectionName));

        services.AddHostedService<OllamaWarmupService>();
        services.AddHttpClient<IOllamaClient, OllamaClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OllamaSettings>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;

    }
}