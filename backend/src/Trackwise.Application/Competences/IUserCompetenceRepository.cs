using Trackwise.Domain.Entities;

namespace Trackwise.Application.Competences;

public interface IUserCompetenceRepository
{
    Task<List<UserCompetence>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default);
    Task<UserCompetence?> GetAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default);
    void Add(UserCompetence link);
    void Remove(UserCompetence link);
}