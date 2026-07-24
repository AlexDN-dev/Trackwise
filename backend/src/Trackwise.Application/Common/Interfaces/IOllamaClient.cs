namespace Trackwise.Application.Common.Interfaces;

public interface IOllamaClient
{
    Task<string?> GenerateJsonAsync(string systemPrompt, string userPrompt,
        CancellationToken cancellationToken = default);
}