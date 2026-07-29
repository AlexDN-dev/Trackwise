using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Scalar.AspNetCore;
using Serilog;
using Trackwise.API.Extensions;
using Trackwise.API.Middleware;
using Trackwise.Infrastructure;
using Trackwise.Application;
using Trackwise.Domain.Enums;
using Trackwise.API.Logging;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With<RedactionEnricher>());
    
    builder.Services.AddControllers();
    builder.Services.AddMemoryCache();
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        
        options.AddPolicy("Premium", policy =>
            policy.RequireClaim("plan", nameof(PlanType.Premium)));
        
        options.AddPolicy("Admin", policy => 
            policy.RequireClaim(System.Security.Claims.ClaimTypes.Role,nameof(UserRole.Admin)));
    });
    
    builder.Services.AddOpenApi();
    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext
            =>
        {
            var key = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                      ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10)
            });
        });

        options.AddPolicy("auth", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromSeconds(30)
                }));
        
        options.AddPolicy("ai", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.FindFirst("sub")?.Value
                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });

    var app = builder.Build();
    
    Log.Information("Provider IA actif : {Provider}", app.Configuration["Ai:Provider"] ?? "Ollama");

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseMiddleware<BannedIpMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer")).AllowAnonymous();
    }
    else
    {
        app.UseHsts();
    }
    
    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "trackwise API a échoué au démarrage");
}
finally
{
    Log.CloseAndFlush();
}