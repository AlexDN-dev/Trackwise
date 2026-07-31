using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trackwise.Application.Admin;
using Trackwise.Application.AI;
using Trackwise.Application.Auth;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Competences;
using Trackwise.Application.Entreprises;
using Trackwise.Application.Subscriptions;
using Trackwise.Infrastructure.AI;
using Trackwise.Infrastructure.Authentication;
using Trackwise.Infrastructure.Payments;
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

        services.AddScoped<IAdminAuditRepository, AdminAuditRepository>();

        var aiProvider = configuration["Ai:Provider"] ?? "Ollama";

        services.Configure<OllamaSettings>(configuration.GetSection(OllamaSettings.SectionName));
        services.Configure<AzureOpenAiSettings>(configuration.GetSection(AzureOpenAiSettings.SectionName));

        if (aiProvider.Equals("AzureOpenAi", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IAiAnalysisClient, AzureOpenAiClient>();
        }
        else
        {
            services.AddHostedService<OllamaWarmupService>();
            services.AddHttpClient<IAiAnalysisClient, OllamaClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OllamaSettings>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
        }

        services.AddScoped<ICompetenceRepository, CompetenceRepository>();
        services.AddScoped<IUserCompetenceRepository, UserCompetenceRepository>();

        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
        
        services.AddScoped<IAdminStatsRepository, AdminStatsRepository>();
        services.AddScoped<IBannedIpRepository, BannedIpRepository>();
        
        services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();
        services.Configure<Email.EmailSettings>(configuration.GetSection(Email.EmailSettings.SectionName));
        // Email:Provider = "Dev" → sender de capture (tests locaux), sinon Brevo (prod).
        if (string.Equals(configuration["Email:Provider"], "Dev", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IEmailSender, Email.DevFileEmailSender>();
        else
            services.AddScoped<IEmailSender, Email.BrevoEmailSender>();
        return services;

    }
}