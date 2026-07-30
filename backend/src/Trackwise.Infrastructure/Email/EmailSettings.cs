namespace Trackwise.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = null!;   // login SMTP Brevo
    public string SmtpKey { get; set; } = null!;    // clé SMTP Brevo (SECRET → env Dokploy)
    public string FromEmail { get; set; } = null!;  // ex: no-reply@trackwise.be
    public string FromName { get; set; } = "Trackwise";
    public string AppBaseUrl { get; set; } = "https://trackwise.be";
}