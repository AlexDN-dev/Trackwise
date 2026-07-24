using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trackwise.Application.AI.Dtos;
using Trackwise.Application.Common.Interfaces;

namespace Trackwise.Application.AI.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IOllamaClient _ollama;
    private readonly ILogger<AnalysisService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    private const string SystemPrompt = """
                                        Tu es un extracteur d'informations d'offres d'emploi.
                                        Extrait UNIQUEMENT ce qui est EXPLICITEMENT mentionne dans l'offre. N'INVENTE RIEN.

                                        Reponds STRICTEMENT en JSON avec ces champs exacts :
                                        {
                                          "title": string | null,           // intitule du poste
                                          "entrepriseName": string | null,  // nom de l'entreprise qui recrute
                                          "location": string | null,        // ville / region
                                          "contractType": 1 | 2 | 3 | null, // 1=CDI, 2=CDD, 3=Interim ; null si non precise
                                          "urlOffer": string | null,        // URL presente dans le texte, sinon null
                                          "description": string | null,     // resume du poste en 1-2 phrases (max 200 caracteres)
                                          "competences": string[]           // technos/competences demandees (courtes, ex: "C#", ".NET", "Angular")
                                        }

                                        Regles strictes :
                                        - Un champ non trouve = null (ou [] pour competences).
                                        - Ne renvoie AUCUN texte en dehors du JSON.
                                        - Les competences sont TOUJOURS des noms courts, pas des phrases.
                                        """;

    public AnalysisService(IOllamaClient ollama, ILogger<AnalysisService> logger)
    {
        _ollama = ollama;
        _logger = logger;
    }

    public async Task<AnalysisResult?> AnalyzeAsync(string text, CancellationToken cancellationToken = default)
    {
        var userPrompt = $"Offer d'emploi : \n\n{text}";

        var raw = await _ollama.GenerateJsonAsync(SystemPrompt, userPrompt, cancellationToken);
        if (string.IsNullOrWhiteSpace(raw))
        {
            _logger.LogWarning("Analyse IA : reponse Ollama vide");
            return null;
        }

        AnalysisResult? result;
        try
        {
            result = JsonSerializer.Deserialize<AnalysisResult>(raw, JsonOpts);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Analyse IA : JSON invalide - {Raw}", raw);
            return null;
        }

        if (result is null ||
            (string.IsNullOrWhiteSpace(result.Title) && string.IsNullOrWhiteSpace(result.EntrepriseName)))
        {
            _logger.LogInformation("Analyse IA : extraction trop vide (ni titre ni entreprise");
            return null;
        }

        result.Competences = result.Competences
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(15)
            .ToList();

        return result;
    }
}