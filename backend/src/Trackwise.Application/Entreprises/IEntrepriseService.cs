using Trackwise.Application.Entreprises.Dtos;

namespace Trackwise.Application.Entreprises;

public interface IEntrepriseService
{
    Task<List<EntrepriseResponse>> SearchAsync(string? term, CancellationToken cancellationToken = default);
}