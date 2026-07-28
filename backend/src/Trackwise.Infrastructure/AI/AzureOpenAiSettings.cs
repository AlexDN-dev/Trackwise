namespace Trackwise.Infrastructure.AI;

public class AzureOpenAiSettings
{
    public const string SectionName = "AzureOpenAi";

    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int MaxOutputTokens { get; set; } = 5000;
    public int MaxInputChars { get; set; } = 3000;
}