using Microsoft.Extensions.DependencyInjection;
using Trackwise.Application.AI;
using Trackwise.Application.AI.Services;
using Trackwise.Application.Auth;
using Trackwise.Application.Auth.Services;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Services;
using Trackwise.Application.Competences;
using Trackwise.Application.Competences.Services;
using Trackwise.Application.Entreprises;
using Trackwise.Application.Entreprises.Services;
using Trackwise.Application.Users;
using Trackwise.Application.Users.Services;

namespace Trackwise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICandidatureService, CandidatureService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IEntrepriseService, EntrepriseService>();

        services.AddScoped<IAnalysisService, AnalysisService>();

        services.AddScoped<IUserCompetenceService, UserCompetenceService>();

        services.AddScoped<IUserService, UserService>();
        return services;
    }
}