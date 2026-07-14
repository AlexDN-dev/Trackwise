using Trackwise.Domain.Entities;

namespace Trackwise.Application.Candidatures;

public interface ICandidatureRepository
{
    Task<List<Candidature>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Candidature?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    void Add(Candidature candidature);
    void Update(Candidature candidature);
    void Remove(Candidature candidature);
}