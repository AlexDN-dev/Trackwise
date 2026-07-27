using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Competences;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class UserCompetenceRepository : IUserCompetenceRepository
{
    private readonly AppDbContext _context;

    public UserCompetenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<UserCompetence>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.UserCompetences
            .AsNoTracking()
            .Include(uc => uc.Competence)
            .Where(uc => uc.UserId == userId)
            .OrderBy(uc => uc.Competence.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default)
        => _context.UserCompetences.AnyAsync(uc => uc.UserId == userId && uc.CompetenceId == competenceId, cancellationToken);

    public Task<UserCompetence?> GetAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default)
        => _context.UserCompetences.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompetenceId == competenceId, cancellationToken);

    public void Add(UserCompetence link)
        => _context.UserCompetences.Add(link);

    public void Remove(UserCompetence link)
        => _context.UserCompetences.Remove(link);
}