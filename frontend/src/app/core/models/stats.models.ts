import { CandidatureStatus } from './candidature.models';

export interface ByStatusDto {
  status: CandidatureStatus;
  count: number;
}

export interface TopEntrepriseDto {
  name: string;
  count: number;
}

export interface FreeStatsResponse {
  totalCandidatures: number;
  inProgress: number;
  interviewsObtained: number;
  positiveResponses: number;
  interviewRate: number | null;
  successRate: number | null;
  byStatus: ByStatusDto[];
  thisWeekNew: number;
  lastWeekNew: number;
  thisMonthNew: number;
  topEntreprises: TopEntrepriseDto[];
}

export interface MonthlyPointDto {
  year: number;
  month: number;
  total: number;
  interviews: number;
  positive: number;
  negative: number;
}

export interface ScoreBucketDto {
  from: number;
  to: number;
  count: number;
}

export interface CompetenceUsageDto {
  name: string;
  count: number;
}

export interface PremiumStatsResponse {
  monthlyEvolution: MonthlyPointDto[];
  averageScore: number | null;
  avgScoreWithInterview: number | null;
  avgScoreWithoutResponse: number | null;
  scoreDistribution: ScoreBucketDto[];
  mostRequestedCompetences: CompetenceUsageDto[];
  mostMissingCompetences: CompetenceUsageDto[];
}
