using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trackwise.Application.Common.Interfaces;

namespace Trackwise.Infrastructure.AI;

public class OllamaWarmupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OllamaWarmupService> _logger;

    public OllamaWarmupService(IServiceProvider services, ILogger<OllamaWarmupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        
        _logger.LogInformation("Warm-up Ollama : chargement du modele en RAM...");

        try
        {
            using var scope = _services.CreateScope();
            var ollama = scope.ServiceProvider.GetRequiredService<IOllamaClient>();

            await ollama.GenerateJsonAsync(
                systemPrompt: "Reponds en JSON.",
                userPrompt: "Renvoie {\"ok\": true}",
                cancellationToken: stoppingToken);

            _logger.LogInformation("Warm-up Ollama : modele chaud, pret a servir");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warp-up Ollama echoue (non bloquant)");
        }
    }
}