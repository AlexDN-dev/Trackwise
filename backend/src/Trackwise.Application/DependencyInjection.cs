using Microsoft.Extensions.DependencyInjection;
using Trackwise.Application.Auth;
using Trackwise.Application.Auth.Services;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Services;

namespace Trackwise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICandidatureService, CandidatureService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}