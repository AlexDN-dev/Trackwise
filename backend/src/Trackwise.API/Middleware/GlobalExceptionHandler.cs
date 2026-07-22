using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Trackwise.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception non geree : {Message}", exception.Message);

        var (status, title) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Non autorise."),
            KeyNotFoundException        => (StatusCodes.Status404NotFound, "Ressource introuvable."),
            _                           => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.")
        };

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = _environment.IsDevelopment() ? exception.Message : null
            }
        });
    }
}