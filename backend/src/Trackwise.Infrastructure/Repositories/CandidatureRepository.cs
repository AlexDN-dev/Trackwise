using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Candidatures;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class CandidatureRepository : ICandidatureRepository
{
    private readonly AppDbContext _context;

    public CandidatureRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Candidature>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Candidatures.Where(c => c.UserId == userId)
            .Include(c => c.Entreprise)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Candidature?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Candidatures
            .Include(c => c.Entreprise)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);    }

    public void Add(Candidature candidature)
    {
        _context.Candidatures.Add(candidature);
    }

    public void Update(Candidature candidature)
    {
        _context.Candidatures.Update(candidature);
    }

    public void Remove(Candidature candidature)
    {
        _context.Candidatures.Remove(candidature);
    }
}