using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Trackwise.Application.Common.Interfaces;

namespace Trackwise.Infrastructure.Email;

public class BrevoEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(IOptions<EmailSettings> settings, ILogger<BrevoEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(string toEmail, string firstname, string token, CancellationToken cancellationToken = default)
    {
        var url = $"{_settings.AppBaseUrl}/confirmer-email?token={token}";
        var html = Wrap(
            $"Bienvenue sur Trackwise, {Escape(firstname)} 👋",
            "Il ne reste qu'une étape : confirme ton adresse email pour activer ton compte.",
            "Confirmer mon email", url,
            "Ce lien est valable 48 heures. Si tu n'es pas à l'origine de cette inscription, ignore simplement ce message.");
        return SendAsync(toEmail, "Confirme ton compte Trackwise", html, cancellationToken);
    }

    public Task SendPasswordResetAsync(string toEmail, string firstname, string token, CancellationToken cancellationToken = default)
    {
        var url = $"{_settings.AppBaseUrl}/reinitialiser-mot-de-passe?token={token}";
        var html = Wrap(
            $"Réinitialisation de ton mot de passe",
            $"Bonjour {Escape(firstname)}, tu as demandé à réinitialiser ton mot de passe. Clique sur le bouton ci-dessous pour en choisir un nouveau.",
            "Choisir un nouveau mot de passe", url,
            "Ce lien est valable 1 heure. Si tu n'es pas à l'origine de cette demande, ignore ce message : ton mot de passe reste inchangé.");
        return SendAsync(toEmail, "Réinitialise ton mot de passe Trackwise", html, cancellationToken);
    }

    public Task SendPasswordChangedAsync(string toEmail, string firstname, CancellationToken cancellationToken = default)
    {
        var html = Wrap(
            "Ton mot de passe a été modifié",
            $"Bonjour {Escape(firstname)}, nous te confirmons que le mot de passe de ton compte Trackwise vient d'être changé.",
            null, null,
            "Si tu n'es pas à l'origine de ce changement, réinitialise immédiatement ton mot de passe et contacte-nous à alexdeneve@hotmail.be.");
        return SendAsync(toEmail, "Ton mot de passe Trackwise a été modifié", html, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpKey, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email envoyé à {Email} : {Subject}", toEmail, subject);
    }

    private static string Wrap(string title, string intro, string? cta, string? ctaUrl, string footer)
    {
        var button = cta is not null && ctaUrl is not null
            ? $@"<tr><td style=""padding:8px 0 24px;"">
                   <a href=""{ctaUrl}"" style=""display:inline-block;background:#5b53e8;color:#ffffff;text-decoration:none;
                      padding:14px 28px;border-radius:10px;font-weight:600;font-size:15px;"">{cta}</a>
                 </td></tr>
                 <tr><td style=""font-size:13px;color:#6a6e7b;padding-bottom:16px;word-break:break-all;"">
                   Ou copie ce lien : <br>{ctaUrl}
                 </td></tr>"
            : "";

        return $@"<!doctype html>
<html><body style=""margin:0;background:#f4f5fb;font-family:Segoe UI,Arial,sans-serif;color:#1b1c22;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f5fb;padding:32px 16px;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""480"" cellpadding=""0"" cellspacing=""0""
             style=""background:#ffffff;border-radius:16px;padding:32px;border:1px solid #e8eaf0;"">
        <tr><td style=""font-weight:700;font-size:20px;color:#5b53e8;padding-bottom:20px;"">Trackwise</td></tr>
        <tr><td style=""font-size:19px;font-weight:600;padding-bottom:12px;"">{title}</td></tr>
        <tr><td style=""font-size:15px;line-height:1.6;color:#3a3d47;padding-bottom:20px;"">{intro}</td></tr>
        {button}
        <tr><td style=""font-size:13px;line-height:1.5;color:#9a9daa;border-top:1px solid #e8eaf0;padding-top:16px;"">{footer}</td></tr>
      </table>
      <div style=""font-size:12px;color:#b5b8c4;padding-top:16px;"">Trackwise · Suivi de candidatures</div>
    </td></tr>
  </table>
</body></html>";
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}