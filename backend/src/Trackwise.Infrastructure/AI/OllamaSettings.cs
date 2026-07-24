namespace Trackwise.Infrastructure.AI;

public class OllamaSettings
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string KeepAlive { get; set; } = "10m";
    public double Temperature { get; set; } = 0.1;
    public int MaxOutputTokens { get; set; } = 400;
    public int MaxInputChars { get; set; } = 3000;
    public int TimeoutSeconds { get; set; } = 45;
}