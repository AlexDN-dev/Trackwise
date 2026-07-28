using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Trackwise.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Trackwise.Application.AI;

namespace Trackwise.Infrastructure.AI;

public class OllamaClient : IAiAnalysisClient
{
    private readonly HttpClient _http;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(HttpClient http, IOptions<OllamaSettings> settings, ILogger<OllamaClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> GenerateJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (userPrompt.Length > _settings.MaxInputChars)
            userPrompt = userPrompt[.._settings.MaxInputChars];

        var payload = new
        {
            model = _settings.Model,
            system = systemPrompt,
            prompt = userPrompt,
            format = "json",
            stream = false,
            keep_alive = _settings.KeepAlive,
            options = new
            {
                temperature = _settings.Temperature,
                num_predict = _settings.MaxOutputTokens
            }
        };

        try
        {
            var response = await _http.PostAsJsonAsync("api/generate", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama API a repondu {StatusCode}", response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
            return body?.Response;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Timeout Ollama après {Seconds}s", _settings.TimeoutSeconds);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Echec appel Ollama");
            return null;
        }
    }

    private sealed record OllamaGenerateResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("response")]
        string? Response);
}