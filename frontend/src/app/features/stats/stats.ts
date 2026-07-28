import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { StatsService } from '../../core/services/stats.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { FreeStatsResponse, PremiumStatsResponse } from '../../core/models/stats.models';
import { CandidatureStatus } from '../../core/models/candidature.models';
import { PlanType } from '../../core/models/user.models';
import { AiQuotaBadge } from '../../shared/components/ai-quota-badge/ai-quota-badge';

interface DonutSegment {
  status: CandidatureStatus;
  label: string;
  color: string;
  count: number;
  percent: number;
  dashArray: string;
  dashOffset: number;
}

const STATUS_META: Record<CandidatureStatus, { label: string; color: string }> = {
  [CandidatureStatus.ToSend]: { label: 'À postuler', color: '#8b90a0' },
  [CandidatureStatus.Sent]: { label: 'Envoyée', color: '#3b82f6' },
  [CandidatureStatus.GotInterview]: { label: 'Entretien', color: '#f0913a' },
  [CandidatureStatus.PositiveResponse]: { label: 'Offre reçue', color: '#12b981' },
  [CandidatureStatus.NegativeResponse]: { label: 'Refusée', color: '#e2557c' },
};

// Fake premium data pour l'aperçu flouté
const FAKE_PREMIUM: PremiumStatsResponse = {
  monthlyEvolution: [
    { year: 2026, month: 2, total: 8, interviews: 2, positive: 1, negative: 2 },
    { year: 2026, month: 3, total: 12, interviews: 4, positive: 1, negative: 3 },
    { year: 2026, month: 4, total: 9, interviews: 3, positive: 2, negative: 2 },
    { year: 2026, month: 5, total: 15, interviews: 5, positive: 2, negative: 4 },
    { year: 2026, month: 6, total: 11, interviews: 4, positive: 1, negative: 3 },
    { year: 2026, month: 7, total: 18, interviews: 6, positive: 3, negative: 4 },
  ],
  averageScore: 62,
  avgScoreWithInterview: 78,
  avgScoreWithoutResponse: 45,
  scoreDistribution: [
    { from: 0, to: 10, count: 2 },
    { from: 10, to: 20, count: 4 },
    { from: 20, to: 30, count: 6 },
    { from: 30, to: 40, count: 8 },
    { from: 40, to: 50, count: 11 },
    { from: 50, to: 60, count: 14 },
    { from: 60, to: 70, count: 12 },
    { from: 70, to: 80, count: 9 },
    { from: 80, to: 90, count: 6 },
    { from: 90, to: 100, count: 3 },
  ],
  mostRequestedCompetences: [
    { name: 'TypeScript', count: 24 }, { name: 'Angular', count: 18 },
    { name: 'C#', count: 15 }, { name: 'SQL', count: 12 }, { name: 'Docker', count: 10 },
  ],
  mostMissingCompetences: [
    { name: 'Kubernetes', count: 12 }, { name: 'AWS', count: 8 },
    { name: 'GraphQL', count: 6 }, { name: 'Terraform', count: 4 }, { name: 'Redis', count: 3 },
  ],
};

@Component({
  selector: 'app-stats',
  imports: [RouterLink, AiQuotaBadge],
  templateUrl: './stats.html',
  styleUrl: './stats.css',
})
export class Stats implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly statsService = inject(StatsService);
  private readonly subscriptionService = inject(SubscriptionService);

  readonly free = signal<FreeStatsResponse | null>(null);
  readonly premium = signal<PremiumStatsResponse | null>(null);
  readonly isPremiumUser = signal(false);
  readonly loading = signal(true);

  readonly mobileMenuOpen = signal(false);

  readonly premiumOrFake = computed(() => this.premium() ?? FAKE_PREMIUM);

  readonly weekTrend = computed(() => {
    const f = this.free();
    if (!f) return null;
    const delta = f.thisWeekNew - f.lastWeekNew;
    if (delta === 0) return { sign: '=', value: 0 };
    return { sign: delta > 0 ? '↑' : '↓', value: Math.abs(delta) };
  });

  readonly donutSegments = computed<DonutSegment[]>(() => {
    const f = this.free();
    if (!f) return [];
    const total = f.byStatus.reduce((s, x) => s + x.count, 0);
    if (total === 0) return [];
    const circumference = 2 * Math.PI * 60;
    let cumulative = 0;
    return f.byStatus
      .filter((s) => s.count > 0)
      .map((s) => {
        const percent = (s.count / total) * 100;
        const dash = (percent / 100) * circumference;
        const segment: DonutSegment = {
          status: s.status,
          label: STATUS_META[s.status].label,
          color: STATUS_META[s.status].color,
          count: s.count,
          percent: Math.round(percent),
          dashArray: `${dash} ${circumference - dash}`,
          dashOffset: -((cumulative / 100) * circumference),
        };
        cumulative += percent;
        return segment;
      });
  });

  readonly maxMonthlyValue = computed(() => {
    return Math.max(1, ...this.premiumOrFake().monthlyEvolution.map((m) => m.total));
  });

  readonly maxScoreBucket = computed(() => {
    return Math.max(1, ...this.premiumOrFake().scoreDistribution.map((b) => b.count));
  });

  readonly monthLabels = [
    'Jan',
    'Fév',
    'Mar',
    'Avr',
    'Mai',
    'Juin',
    'Juil',
    'Août',
    'Sep',
    'Oct',
    'Nov',
    'Déc',
  ];

  monthLabel(m: { year: number; month: number }): string {
    return this.monthLabels[m.month - 1];
  }

  ngOnInit(): void {
    this.subscriptionService.getMine().subscribe({
      next: (sub) => {
        const isPremium = sub.plan === PlanType.Premium && sub.isActive;
        this.isPremiumUser.set(isPremium);
        this.statsService.getFree().subscribe({
          next: (f) => {
            this.free.set(f);
            if (isPremium) {
              this.statsService.getPremium().subscribe({
                next: (p) => {
                  this.premium.set(p);
                  this.loading.set(false);
                },
                error: () => this.loading.set(false),
              });
            } else {
              this.loading.set(false);
            }
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/auth']));
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  goPremium(): void {
    this.router.navigate(['/profil/abonnement']);
  }

  formatRate(v: number | null): string {
    return v === null ? '—' : `${Math.round(v * 100)}%`;
  }

  formatScore(v: number | null): string {
    return v === null ? '—' : `${Math.round(v)}%`;
  }
}
