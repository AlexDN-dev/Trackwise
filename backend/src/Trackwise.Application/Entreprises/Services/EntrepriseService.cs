using Trackwise.Application.Entreprises.Dtos;

namespace Trackwise.Application.Entreprises.Services;

public class EntrepriseService : IEntrepriseService
{
    private readonly IEntrepriseRepository _repository;

    public EntrepriseService(IEntrepriseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EntrepriseResponse>> SearchAsync(string? term, CancellationToken cancellationToken = default)
    {
        var entreprises = await _repository.SearchAsync(term, cancellationToken);
        return entreprises.Select(e => new EntrepriseResponse
        {
            Id = e.Id, Name = e.Name, Sector = e.Sector, City = e.City, Website = e.Website
        }).ToList();
    }
}