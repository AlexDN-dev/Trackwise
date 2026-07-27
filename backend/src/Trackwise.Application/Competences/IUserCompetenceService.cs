using Trackwise.Application.Competences.Dtos;

namespace Trackwise.Application.Competences;

public interface IUserCompetenceService
{
    Task<List<CompetenceResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CompetenceResponse> AddAsync(Guid userId, AddUserCompetenceRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid userId, Guid competenceId, CancellationToken cancellationToken = default);
    Task<List<CompetenceResponse>> SearchAsync(string? term, int limit, CancellationToken cancellationToken = default);
}