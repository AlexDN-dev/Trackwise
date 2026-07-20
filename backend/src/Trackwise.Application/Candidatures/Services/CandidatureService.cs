using Trackwise.Application.Candidatures.Dtos;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;
using Trackwise.Application.Entreprises;

namespace Trackwise.Application.Candidatures.Services;

public class CandidatureService : ICandidatureService
{
    private readonly ICandidatureRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntrepriseRepository _entrepriseRepository;

    public CandidatureService(ICandidatureRepository repository, IUnitOfWork unitOfWork, IEntrepriseRepository entrepriseRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _entrepriseRepository = entrepriseRepository;
    }

    public async Task<List<CandidatureResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var candidatures = await _repository.GetAllByUserAsync(userId, cancellationToken);

        return candidatures.Select(ToResponse).ToList();
    }

    public async Task<CandidatureResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        return candidature is null ? null : ToResponse(candidature);
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

        _repository.Add(candidature);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(candidature);
    }
    private async Task<Guid> ResolveEntrepriseAsync(CreateCandidatureRequest request, CancellationToken cancellationToken)
    {
        if (request.EntrepriseId is Guid pickedId)
            return pickedId;            

        var name = request.EntrepriseName!.Trim();       
        var normalized = name.ToLowerInvariant();

        var existing = await _entrepriseRepository.GetByNormalizedNameAsync(normalized, cancellationToken);
        if (existing is not null)
            return existing.Id;                          

        var entreprise = new Entreprise { Name = name, NormalizedName = normalized };
        _entrepriseRepository.Add(entreprise);          
        return entreprise.Id;                             
    }

    public async Task<CandidatureResponse?> UpdateAsync(Guid id, Guid userId, UpdateCandidatureRequest request, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (candidature is null)
            return null;

        candidature.Title = request.Title;
        candidature.Location = request.Location;
        candidature.UrlOffer = request.UrlOffer;
        candidature.Description = request.Description;
        candidature.ContractType = request.ContractType;
        candidature.Status = request.Status;
        candidature.PostulationDate = request.PostulationDate;
        candidature.EntrepriseId = request.EntrepriseId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(candidature);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var candidature = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (candidature is null)
            return false;

        _repository.Remove(candidature);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    
    
    private static CandidatureResponse ToResponse(Candidature c) => new()
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
        EntrepriseName = c.Entreprise?.Name
    };
}