using Trackwise.Application.Candidatures.Dtos;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Competences;
using Trackwise.Application.Competences.Dtos;
using Trackwise.Application.Entreprises;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Candidatures.Services;

public class CandidatureService : ICandidatureService
{
    private readonly ICandidatureRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntrepriseRepository _entrepriseRepository;
    private readonly ICompetenceRepository _competenceRepository;
    private readonly IUserCompetenceRepository _userCompetenceRepository;

    public CandidatureService(
        ICandidatureRepository repository,
        IUnitOfWork unitOfWork,
        IEntrepriseRepository entrepriseRepository,
        ICompetenceRepository competenceRepository,
        IUserCompetenceRepository userCompetenceRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _entrepriseRepository = entrepriseRepository;
        _competenceRepository = competenceRepository;
        _userCompetenceRepository = userCompetenceRepository;
    }

    public async Task<List<CandidatureResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var candidatures = await _repository.GetAllByUserAsync(userId, cancellationToken);
        var userSkillIds = await GetUserSkillIdsAsync(userId, cancellationToken);
        return candidatures.Select(c => ToResponse(c, userSkillIds)).ToList();
    }

    public async Task<CandidatureResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (candidature is null) return null;
        var userSkillIds = await GetUserSkillIdsAsync(userId, cancellationToken);
        return ToResponse(candidature, userSkillIds);
    }

    public async Task<CandidatureResponse> CreateAsync(Guid userId, CreateCandidatureRequest request, CancellationToken cancellationToken = default)
    {
        var entrepriseId = await ResolveEntrepriseAsync(request, cancellationToken);

        var candidature = new Candidature
        {
            Title = request.Title,
            Location = request.Location,
            UrlOffer = request.UrlOffer,
            Description = request.Description,
            ContractType = request.ContractType,
            PostulationDate = request.PostulationDate,
            EntrepriseId = entrepriseId,
            UserId = userId,
            Status = CandidatureStatus.ToSend
        };

        if (request.Competences is { Count: > 0 })
        {
            var competences = await ResolveCompetencesAsync(request.Competences, cancellationToken);
            foreach (var comp in competences) candidature.Competences.Add(comp);
        }

        _repository.Add(candidature);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userSkillIds = await GetUserSkillIdsAsync(userId, cancellationToken);
        return ToResponse(candidature, userSkillIds);
    }

    public async Task<CandidatureResponse?> UpdateAsync(Guid id, Guid userId, UpdateCandidatureRequest request, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (candidature is null) return null;

        candidature.Title = request.Title;
        candidature.Location = request.Location;
        candidature.UrlOffer = request.UrlOffer;
        candidature.Description = request.Description;
        candidature.ContractType = request.ContractType;
        candidature.Status = request.Status;
        candidature.PostulationDate = request.PostulationDate;
        candidature.EntrepriseId = request.EntrepriseId;

        if (request.Competences is not null)
        {
            candidature.Competences.Clear();
            if (request.Competences.Count > 0)
            {
                var competences = await ResolveCompetencesAsync(request.Competences, cancellationToken);
                foreach (var comp in competences) candidature.Competences.Add(comp);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userSkillIds = await GetUserSkillIdsAsync(userId, cancellationToken);
        return ToResponse(candidature, userSkillIds);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (candidature is null) return false;

        _repository.Remove(candidature);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Guid> ResolveEntrepriseAsync(CreateCandidatureRequest request, CancellationToken cancellationToken)
    {
        if (request.EntrepriseId is Guid pickedId) return pickedId;

        var name = request.EntrepriseName!.Trim();
        var normalized = name.ToLowerInvariant();

        var existing = await _entrepriseRepository.GetByNormalizedNameAsync(normalized, cancellationToken);
        if (existing is not null) return existing.Id;

        var entreprise = new Entreprise { Name = name, NormalizedName = normalized };
        _entrepriseRepository.Add(entreprise);
        return entreprise.Id;
    }

    private async Task<List<Competence>> ResolveCompetencesAsync(IEnumerable<string> names, CancellationToken cancellationToken)
    {
        var result = new List<Competence>();
        var seen = new HashSet<string>();

        foreach (var raw in names)
        {
            var name = raw?.Trim();
            if (string.IsNullOrEmpty(name)) continue;
            var normalized = name.ToLowerInvariant();
            if (!seen.Add(normalized)) continue;

            var existing = await _competenceRepository.GetByNormalizedNameAsync(normalized, cancellationToken);
            if (existing is not null)
            {
                result.Add(existing);
            }
            else
            {
                var newComp = new Competence { Name = name, NormalizedName = normalized };
                _competenceRepository.Add(newComp);
                result.Add(newComp);
            }
        }

        return result;
    }

    private async Task<HashSet<Guid>> GetUserSkillIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var links = await _userCompetenceRepository.GetForUserAsync(userId, cancellationToken);
        return links.Select(l => l.CompetenceId).ToHashSet();
    }

    private static byte? ComputeScore(Candidature c, HashSet<Guid> userSkillIds)
    {
        if (c.Competences.Count == 0) return null;
        var matched = c.Competences.Count(comp => userSkillIds.Contains(comp.Id));
        return (byte)Math.Round(matched * 100.0 / c.Competences.Count);
    }

    private static CandidatureResponse ToResponse(Candidature c, HashSet<Guid> userSkillIds) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Status = c.Status,
        Location = c.Location,
        UrlOffer = c.UrlOffer,
        Description = c.Description,
        ContractType = c.ContractType,
        PostulationDate = c.PostulationDate,
        CreatedAt = c.CreatedAt,
        EntrepriseId = c.EntrepriseId,
        EntrepriseName = c.Entreprise?.Name,
        Competences = c.Competences
            .OrderBy(comp => comp.Name)
            .Select(comp => new CompetenceResponse { Id = comp.Id, Name = comp.Name })
            .ToList(),
        Score = ComputeScore(c, userSkillIds),
        MissingCompetences = c.Competences
            .Where(comp => !userSkillIds.Contains(comp.Id))
            .OrderBy(comp => comp.Name)
            .Select(comp => comp.Name)
            .ToList(),
    };
}