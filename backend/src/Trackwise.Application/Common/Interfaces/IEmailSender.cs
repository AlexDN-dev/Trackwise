namespace Trackwise.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string toEmail, string firstname, string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(string toEmail, string firstname, string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordChangedAsync(string toEmail, string firstname, CancellationToken cancellationToken = default);
}