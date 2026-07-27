using Trackwise.Domain.Entities;

namespace Trackwise.Application.Competences;

public interface ICompetenceRepository
{
    Task<Competence?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
    Task<List<Competence>> SearchAsync(string? term, int limit, CancellationToken cancellationToken = default);
    void Add(Competence competence);
}