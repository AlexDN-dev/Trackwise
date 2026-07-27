import { Component, computed, signal } from '@angular/core';

type Plan = 'Free' | 'Premium';

@Component({
  selector: 'app-profile-subscription',
  imports: [],
  templateUrl: './subscription.html',
  styleUrl: './subscription.css',
})
export class ProfileSubscription {
  // Placeholder — à brancher sur GET /api/users/me/subscription
  readonly plan = signal<Plan>('Free');
  readonly premiumUntil = signal<string | null>(null);

  readonly isFree = computed(() => this.plan() === 'Free');
  readonly isPremium = computed(() => this.plan() === 'Premium');

  readonly freeFeatures = [
    'Suivi illimité de candidatures',
    'Analyse IA des offres',
    'Score de compatibilité',
    'Statistiques de base',
  ];

  readonly premiumFeatures = [
    'Statistiques avancées & tendances',
    'Comparateur d\'offres',
    'Suggestions IA personnalisées',
    'Export PDF & Excel',
    'Support prioritaire',
  ];

  upgrade(): void {
    // À brancher sur POST /api/subscription/checkout (Stripe)
    console.log('Upgrade to Premium — Stripe checkout à venir');
  }

  manage(): void {
    // À brancher sur POST /api/subscription/portal (Stripe portal)
    console.log('Manage subscription — Stripe portal à venir');
  }
}
