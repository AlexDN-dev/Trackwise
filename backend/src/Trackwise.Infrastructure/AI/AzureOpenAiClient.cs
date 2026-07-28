using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Trackwise.Application.AI;

namespace Trackwise.Infrastructure.AI;

public class AzureOpenAiClient : IAiAnalysisClient
{
    private readonly ChatClient _chatClient;
    private readonly AzureOpenAiSettings _settings;
    private readonly ILogger<AzureOpenAiClient> _logger;

    public AzureOpenAiClient(IOptions<AzureOpenAiSettings> settings, ILogger<AzureOpenAiClient> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var azureClient = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new AzureKeyCredential(_settings.ApiKey));

        _chatClient = azureClient.GetChatClient(_settings.Deployment);
    }

    public async Task<string?> GenerateJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (userPrompt.Length > _settings.MaxInputChars)
            userPrompt = userPrompt[.._settings.MaxInputChars];

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _settings.MaxOutputTokens,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            ReasoningEffortLevel = ChatReasoningEffortLevel.Low,
        };
        
#pragma warning disable AOAI001
        options.SetNewMaxCompletionTokensPropertyEnabled(true);
#pragma warning restore AOAI001

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt),
        };

        try
        {
            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content.Count > 0 ? response.Value.Content[0].Text : null;        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Appel Azure OpenAI échoué");
            return null;
        }
    }
}