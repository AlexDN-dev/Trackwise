using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Auth;

public interface IEmailTokenRepository
{
    void Add(EmailToken token);

    Task<EmailToken?> GetByHashAsync(string tokenHash, EmailTokenType type,
        CancellationToken cancellationToken = default);

    Task<List<EmailToken>> GetActiveByUserAsync(Guid userId, EmailTokenType type,
        CancellationToken cancellationToken = default);
}