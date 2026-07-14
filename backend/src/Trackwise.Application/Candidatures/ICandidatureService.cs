using Trackwise.Application.Candidatures.Dtos;

namespace Trackwise.Application.Candidatures;

public interface ICandidatureService
{
    Task<List<CandidatureResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CandidatureResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<CandidatureResponse> CreateAsync(Guid userId, CreateCandidatureRequest request, CancellationToken cancellationToken = default);
    Task<CandidatureResponse?> UpdateAsync(Guid id, Guid userId, UpdateCandidatureRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}