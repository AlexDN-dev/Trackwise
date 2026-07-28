using Trackwise.Application.Auth;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Dtos;
using Trackwise.Application.Stats.Dtos;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Stats.Services;

public class StatsService : IStatsService
{
    private readonly ICandidatureService _candidatureService;
    private readonly IUserRepository _userRepository;

    public StatsService(ICandidatureService candidatureService, IUserRepository userRepository)
    {
        _candidatureService = candidatureService;
        _userRepository = userRepository;
    }

    public async Task<FreeStatsResponse> GetFreeStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var candidatures = await _candidatureService.GetAllAsync(userId, cancellationToken);
        return ComputeFree(candidatures);
    }

    public async Task<PremiumStatsResponse?> GetPremiumStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        var isActive = user.PlanType == PlanType.Premium
                       && user.PremiumUntil.HasValue
                       && user.PremiumUntil > DateTimeOffset.UtcNow;
        if (!isActive) return null;

        var candidatures = await _candidatureService.GetAllAsync(userId, cancellationToken);
        return ComputePremium(candidatures);
    }

    // ---------- FREE ----------

    private static FreeStatsResponse ComputeFree(List<CandidatureResponse> candidatures)
    {
        var total = candidatures.Count;
        var sent = candidatures.Count(c => c.Status >= CandidatureStatus.Sent);
        var interviews = candidatures.Count(c => c.Status == CandidatureStatus.GotInterview || c.Status == CandidatureStatus.PositiveResponse);
        var positive = candidatures.Count(c => c.Status == CandidatureStatus.PositiveResponse);
        var negative = candidatures.Count(c => c.Status == CandidatureStatus.NegativeResponse);
        var inProgress = candidatures.Count(c => c.Status == CandidatureStatus.ToSend || c.Status == CandidatureStatus.Sent || c.Status == CandidatureStatus.GotInterview);

        double? interviewRate = sent > 0 ? (double)interviews / sent : null;
        int responded = positive + negative;
        double? successRate = responded > 0 ? (double)positive / responded : null;

        // Répartition par statut (avec 0 pour ceux non présents)
        var byStatus = Enum.GetValues<CandidatureStatus>()
            .Select(s => new ByStatusDto { Status = s, Count = candidatures.Count(c => c.Status == s) })
            .ToList();

        // Activité récente
        var (weekStart, lastWeekStart, monthStart) = GetPeriodBoundaries();
        var thisWeekNew = candidatures.Count(c => c.CreatedAt >= weekStart);
        var lastWeekNew = candidatures.Count(c => c.CreatedAt >= lastWeekStart && c.CreatedAt < weekStart);
        var thisMonthNew = candidatures.Count(c => c.CreatedAt >= monthStart);

        // Top 5 entreprises
        var topEntreprises = candidatures
            .Where(c => !string.IsNullOrWhiteSpace(c.EntrepriseName))
            .GroupBy(c => c.EntrepriseName!)
            .Select(g => new TopEntrepriseDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Name)
            .Take(5)
            .ToList();

        return new FreeStatsResponse
        {
            TotalCandidatures = total,
            InProgress = inProgress,
            InterviewsObtained = interviews,
            PositiveResponses = positive,
            InterviewRate = interviewRate,
            SuccessRate = successRate,
            ByStatus = byStatus,
            ThisWeekNew = thisWeekNew,
            LastWeekNew = lastWeekNew,
            ThisMonthNew = thisMonthNew,
            TopEntreprises = topEntreprises,
        };
    }

    // ---------- PREMIUM ----------

    private static PremiumStatsResponse ComputePremium(List<CandidatureResponse> candidatures)
    {
        // Évolution mensuelle : 6 derniers mois
        var now = DateTimeOffset.UtcNow;
        var currentMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthlyEvolution = new List<MonthlyPointDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = currentMonthStart.AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var slice = candidatures.Where(c => c.CreatedAt >= monthStart && c.CreatedAt < monthEnd).ToList();
            monthlyEvolution.Add(new MonthlyPointDto
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                Total = slice.Count,
                Interviews = slice.Count(c => c.Status == CandidatureStatus.GotInterview || c.Status == CandidatureStatus.PositiveResponse),
                Positive = slice.Count(c => c.Status == CandidatureStatus.PositiveResponse),
                Negative = slice.Count(c => c.Status == CandidatureStatus.NegativeResponse),
            });
        }

        // Analytics du score
        var withScore = candidatures.Where(c => c.Score.HasValue).ToList();
        double? averageScore = withScore.Count > 0 ? withScore.Average(c => (double)c.Score!.Value) : null;

        var withInterview = withScore.Where(c => c.Status == CandidatureStatus.GotInterview || c.Status == CandidatureStatus.PositiveResponse).ToList();
        double? avgWithInterview = withInterview.Count > 0 ? withInterview.Average(c => (double)c.Score!.Value) : null;

        var noResponse = withScore.Where(c => c.Status == CandidatureStatus.Sent).ToList();
        double? avgNoResponse = noResponse.Count > 0 ? noResponse.Average(c => (double)c.Score!.Value) : null;

        // Distribution en 10 buckets de 10 points chacun (0-9, 10-19, …, 90-100)
        var buckets = new List<ScoreBucketDto>();
        for (int b = 0; b < 100; b += 10)
        {
            var upper = b + 10;
            var count = withScore.Count(c =>
                c.Score >= b && (b == 90 ? c.Score <= 100 : c.Score < upper));
            buckets.Add(new ScoreBucketDto { From = b, To = upper, Count = count });
        }

        // Top compétences demandées
        var mostRequested = candidatures
            .SelectMany(c => c.Competences)
            .GroupBy(c => c.Name)
            .Select(g => new CompetenceUsageDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .Take(10)
            .ToList();

        // Top compétences manquantes (celles qui reviennent dans les offres et que je n'ai pas)
        var mostMissing = candidatures
            .SelectMany(c => c.MissingCompetences)
            .GroupBy(name => name)
            .Select(g => new CompetenceUsageDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .Take(10)
            .ToList();

        return new PremiumStatsResponse
        {
            MonthlyEvolution = monthlyEvolution,
            AverageScore = averageScore,
            AvgScoreWithInterview = avgWithInterview,
            AvgScoreWithoutResponse = avgNoResponse,
            ScoreDistribution = buckets,
            MostRequestedCompetences = mostRequested,
            MostMissingCompetences = mostMissing,
        };
    }
    
    private static (DateTimeOffset weekStart, DateTimeOffset lastWeekStart, DateTimeOffset monthStart) GetPeriodBoundaries()
    {
        var now = DateTimeOffset.UtcNow;
        // Lundi de cette semaine (culture FR : semaine commence lundi)
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var weekStart = today.AddDays(-daysFromMonday);
        var lastWeekStart = weekStart.AddDays(-7);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return (weekStart, lastWeekStart, monthStart);
    }
}