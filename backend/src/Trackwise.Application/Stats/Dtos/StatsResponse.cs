using Trackwise.Domain.Enums;

namespace Trackwise.Application.Stats.Dtos;

public class FreeStatsResponse
{
    public int TotalCandidatures { get; set; }
    public int InProgress { get; set; }
    public int InterviewsObtained { get; set; }
    public int PositiveResponses { get; set; }
    
    public double? InterviewRate { get; set; }
    public double? SuccessRate { get; set; }

    public List<ByStatusDto> ByStatus { get; set; } = new();
    
    public int ThisWeekNew { get; set; }
    public int LastWeekNew { get; set; }
    public int ThisMonthNew { get; set; }

    public List<TopEntrepriseDto> TopEntreprises { get; set; } = new();
}

public class ByStatusDto
{
    public CandidatureStatus Status { get; set; }
    public int Count { get; set; }
}

public class TopEntrepriseDto
{
    public string Name { get; set; } = null!;
    public int Count { get; set; }
}

public class PremiumStatsResponse
{
    public List<MonthlyPointDto> MonthlyEvolution { get; set; } = new();
    
    public double? AverageScore { get; set; }
    public double? AvgScoreWithInterview { get; set; }
    public double? AvgScoreWithoutResponse { get; set; }
    public List<ScoreBucketDto> ScoreDistribution { get; set; } = new();

    public List<CompetenceUsageDto> MostRequestedCompetences { get; set; } = new();
    public List<CompetenceUsageDto> MostMissingCompetences { get; set; } = null!;
}

public class MonthlyPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Total { get; set; }
    public int Interviews { get; set; }
    public int Positive { get; set; }
    public int Negative { get; set; }
}

public class ScoreBucketDto
{
    public int From { get; set; }
    public int To { get; set; }
    public int Count { get; set; }
}

public class CompetenceUsageDto
{
    public string Name { get; set; } = null!;
    public int Count { get; set; }
}