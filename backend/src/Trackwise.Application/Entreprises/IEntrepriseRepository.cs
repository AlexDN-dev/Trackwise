using Trackwise.Domain.Entities;

namespace Trackwise.Application.Entreprises;

public interface IEntrepriseRepository
{
    Task<List<Entreprise>> SearchAsync(string? term, CancellationToken cancellationToken = default);
    Task<Entreprise?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
    void Add(Entreprise entreprise);
}