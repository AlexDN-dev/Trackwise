using Trackwise.Domain.Enums;

namespace Trackwise.Application.AI.Dtos;

public class AnalysisResult
{
    public string? Title { get; set; }
    public string? EntrepriseName { get; set; }
    public string? Location { get; set; }
    public ContractType? ContractType { get; set; }
    public string? urlOffer { get; set; }
    public string? Description { get; set; }
    public List<string> Competences { get; set; } = new();
}