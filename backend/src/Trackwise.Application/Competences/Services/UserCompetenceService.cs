using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Competences.Dtos;
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Competences.Services;

public class UserCompetenceService : IUserCompetenceService
{
    private readonly ICompetenceRepository _competenceRepository;
    private readonly IUserCompetenceRepository _userCompetenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserCompetenceService(
        ICompetenceRepository competenceRepository,
        IUserCompetenceRepository userCompetenceRepository,
        IUnitOfWork unitOfWork)
    {
        _competenceRepository = competenceRepository;
        _userCompetenceRepository = userCompetenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CompetenceResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var links = await _userCompetenceRepository.GetForUserAsync(userId, cancellationToken);
        return links.Select(l => ToResponse(l.Competence)).ToList();
    }

    public async Task<CompetenceResponse> AddAsync(Guid userId, AddUserCompetenceRequest request, CancellationToken cancellationToken = default)
    {
        var competence = await GetOrCreateCompetenceAsync(request.Name, cancellationToken);

        var alreadyLinked = await _userCompetenceRepository.ExistsAsync(userId, competence.Id, cancellationToken);
        if (!alreadyLinked)
        {
            _userCompetenceRepository.Add(new UserCompetence
            {
                UserId = userId,
                CompetenceId = competence.Id,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(competence);
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default)
    {
        var link = await _userCompetenceRepository.GetAsync(userId, competenceId, cancellationToken);
        if (link is null) return false;

        _userCompetenceRepository.Remove(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<CompetenceResponse>> SearchAsync(string? term, int limit, CancellationToken cancellationToken = default)
    {
        var competences = await _competenceRepository.SearchAsync(term, limit, cancellationToken);
        return competences.Select(ToResponse).ToList();
    }

    private async Task<Competence> GetOrCreateCompetenceAsync(string name, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var normalized = trimmed.ToLowerInvariant();

        var existing = await _competenceRepository.GetByNormalizedNameAsync(normalized, cancellationToken);
        if (existing is not null) return existing;

        var competence = new Competence { Name = trimmed, NormalizedName = normalized };
        _competenceRepository.Add(competence);
        return competence;
    }

    private static CompetenceResponse ToResponse(Competence c) => new()
    {
        Id = c.Id,
        Name = c.Name
    };
}