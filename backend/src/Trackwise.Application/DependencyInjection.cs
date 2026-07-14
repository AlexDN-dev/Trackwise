using Microsoft.Extensions.DependencyInjection;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Services;

namespace Trackwise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICandidatureService, CandidatureService>();
        return services;
    }
}