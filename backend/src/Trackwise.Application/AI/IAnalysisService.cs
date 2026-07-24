using Trackwise.Application.AI.Dtos;

namespace Trackwise.Application.AI;

public interface IAnalysisService
{
    Task<AnalysisResult?> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
}