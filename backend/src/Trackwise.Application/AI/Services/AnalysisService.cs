using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trackwise.Application.AI.Dtos;

namespace Trackwise.Application.AI.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IAiAnalysisClient _ai;
    private readonly ILogger<AnalysisService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
private const string SystemPrompt = """
    Tu es un extracteur d'informations d'offres d'emploi.

    REGLES ABSOLUES :
    - Reponds STRICTEMENT en JSON, aucun texte hors du JSON.
    - Reponds TOUJOURS en FRANCAIS. Traduis systematiquement les intitules et descriptions, meme si l'offre est en anglais, neerlandais, allemand ou toute autre langue.
    - N'invente PAS de faits (ville, entreprise, URL, salaire). Champ non trouve = null.
    - EXCEPTION pour "title" : si l'offre ne cite pas d'intitule explicite, deduis-en un court a partir des technos et du role, EN FRANCAIS.

    CHAMPS ATTENDUS :
    {
      "title": string | null,
        // Intitule du poste EN FRANCAIS, max 80 caracteres.
      "entrepriseName": string | null,
        // Nom OFFICIEL de l'entreprise qui recrute (pas un mot au hasard du texte).
        "location": string | null,
        // Format "Ville, Pays" (ex: "Bruxelles, Belgique", "Paris, France", "Utrecht, Pays-Bas").
        // Ignore les regions/departements/provinces. Si aucun lieu geographique explicite : null.
      "contractType": 1 | 2 | 3 | null,
        // 1=CDI, 2=CDD, 3=Interim ; null si non precise.
      "urlOffer": string | null,
        // URL commencant par http:// ou https:// presente dans le texte. Sinon null.
      "description": string | null,
        // Resume du poste EN FRANCAIS, 3 a 4 phrases, max 400 caracteres. Couvre : missions principales, technos clefs, contexte de l'equipe.
      "competences": string[]
        // Noms COURTS et ORIGINAUX des technos (ne pas traduire) : "C#", ".NET", "Angular", "Python", "PostgreSQL".
    }

    EXEMPLES :

    Offre 1 :
    "Backend Developer at Spotify in Stockholm. Permanent contract. You will build our microservices APIs in Java and Kotlin. Apply at https://spotify.com/careers/be-dev-42"

    JSON 1 :
    {"title":"Développeur Backend","entrepriseName":"Spotify","location":"Stockholm, Suède","contractType":1,"urlOffer":"https://spotify.com/careers/be-dev-42","description":"Développement des APIs microservices de Spotify en Java et Kotlin. Vous rejoindrez l'équipe backend responsable du cœur de la plateforme. Environnement technique moderne et pratiques DevOps solides.","competences":["Java","Kotlin"]}

    Offre 2 :
    "Wij zoeken een enthousiaste .NET developer bij Bol.com. Locatie: Utrecht. Voltijds vast contract. Ervaring met C# en Azure."

    JSON 2 :
    {"title":"Développeur .NET","entrepriseName":"Bol.com","location":"Utrecht, Pays-Bas","contractType":1,"urlOffer":null,"description":"Poste de développeur .NET à temps plein chez Bol.com. Vous travaillerez sur des services C# hébergés sur Azure. Contrat CDI, équipe produit e-commerce.","competences":["C#",".NET","Azure"]}
    """;

    public AnalysisService(IAiAnalysisClient ai, ILogger<AnalysisService> logger)
    {
        _ai = ai;
        _logger = logger;
    }

    public async Task<AnalysisResult?> AnalyzeAsync(string text, CancellationToken cancellationToken = default)
    {
        var userPrompt = $"Offer d'emploi : \n\n{text}";

        var raw = await _ai.GenerateJsonAsync(SystemPrompt, userPrompt, cancellationToken);
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