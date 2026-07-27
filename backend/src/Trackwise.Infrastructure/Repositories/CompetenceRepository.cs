using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Competences;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class CompetenceRepository : ICompetenceRepository
{
    private readonly AppDbContext _context;

    public CompetenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Competence?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        => _context.Competences.FirstOrDefaultAsync(c => c.NormalizedName == normalizedName, cancellationToken);

    public async Task<List<Competence>> SearchAsync(string? term, int limit, CancellationToken cancellationToken = default)
    {
        var query = _context.Competences.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.Trim().ToLowerInvariant();
            query = query.Where(c => c.NormalizedName != null && c.NormalizedName.Contains(normalized));
        }

        return await query
            .OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Add(Competence competence)
        => _context.Competences.Add(competence);
}