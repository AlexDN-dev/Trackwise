using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trackwise.Application.Common.Interfaces;

namespace Trackwise.Infrastructure.Email;

// Sender de DÉVELOPPEMENT : n'envoie aucun vrai email.
// Il capture le token en clair dans un fichier JSONL, lu par la suite de tests Playwright.
// Enregistré uniquement quand Email:Provider = "Dev" (voir DependencyInjection). Jamais en prod.
public class DevFileEmailSender : IEmailSender
{
    private static readonly object Lock = new();
    private readonly string _file;
    private readonly ILogger<DevFileEmailSender> _logger;

    public DevFileEmailSender(IConfiguration configuration, ILogger<DevFileEmailSender> logger)
    {
        _logger = logger;
        _file = configuration["Email:DevCaptureFile"]
                ?? Path.Combine(Path.GetTempPath(), "trackwise-dev-emails.jsonl");
    }

    public Task SendEmailConfirmationAsync(string toEmail, string firstname, string token, CancellationToken cancellationToken = default)
        => Capture(toEmail, "confirmation", token);

    public Task SendPasswordResetAsync(string toEmail, string firstname, string token, CancellationToken cancellationToken = default)
        => Capture(toEmail, "reset", token);

    public Task SendPasswordChangedAsync(string toEmail, string firstname, CancellationToken cancellationToken = default)
        => Capture(toEmail, "password-changed", null);

    private Task Capture(string toEmail, string type, string? token)
    {
        var line = JsonSerializer.Serialize(new { to = toEmail, type, token, sentAt = DateTimeOffset.UtcNow });
        lock (Lock)
        {
            File.AppendAllText(_file, line + Environment.NewLine);
        }
        _logger.LogWarning("[DEV EMAIL] {Type} -> {Email} (token capturé dans {File})", type, toEmail, _file);
        return Task.CompletedTask;
    }
}
