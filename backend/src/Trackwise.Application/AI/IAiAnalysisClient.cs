namespace Trackwise.Application.AI;

public interface IAiAnalysisClient
{
    Task<string?> GenerateJsonAsync(string systemPrompt, string userPrompt,
        CancellationToken cancellationToken = default);
}