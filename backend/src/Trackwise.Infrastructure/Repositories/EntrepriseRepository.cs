using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Entreprises;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class EntrepriseRepository : IEntrepriseRepository
{
    private readonly AppDbContext _context;

    public EntrepriseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Entreprise>> SearchAsync(string? term, CancellationToken cancellationToken = default)
    {
        var query = _context.Entreprises.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(e => e.Name.Contains(term));

        return await query.OrderBy(e => e.Name).Take(20).ToListAsync(cancellationToken);
    }

    public Task<Entreprise?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        return _context.Entreprises.FirstOrDefaultAsync(e => e.NormalizedName == normalizedName, cancellationToken);
    }

    public void Add(Entreprise entreprise)
        => _context.Entreprises.Add(entreprise);
}