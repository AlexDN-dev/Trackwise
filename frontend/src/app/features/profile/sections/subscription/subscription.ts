import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError, of, take, timer } from 'rxjs';
import { SubscriptionService } from '../../../../core/services/subscription.service';
import { SubscriptionResponse } from '../../../../core/models/subscription.models';
import { PlanType } from '../../../../core/models/user.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-profile-subscription',
  imports: [],
  templateUrl: './subscription.html',
  styleUrl: './subscription.css',
})
export class ProfileSubscription implements OnInit {
  private readonly service = inject(SubscriptionService);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);

  readonly subscription = signal<SubscriptionResponse | null>(null);
  readonly loading = signal(true);
  readonly processing = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showSuccessBanner = signal(false);

  readonly isFree = computed(() => this.subscription()?.plan === PlanType.Free);
  readonly isPremium = computed(() => this.subscription()?.plan === PlanType.Premium);

  readonly premiumUntilFmt = computed(() => {
    const until = this.subscription()?.premiumUntil;
    return until
      ? new Date(until).toLocaleDateString('fr-FR', {
          day: 'numeric',
          month: 'long',
          year: 'numeric',
        })
      : null;
  });

  readonly freeFeatures = [
    'Suivi illimité de candidatures',
    'Analyse IA des offres',
    'Score de compatibilité',
    'Statistiques de base',
  ];

  readonly premiumFeatures = [
    'Statistiques avancées & tendances',
    "Comparateur d'offres",
    'Suggestions IA personnalisées',
    'Export PDF & Excel',
    'Support prioritaire',
  ];

  ngOnInit(): void {
    const justPaid = this.route.snapshot.queryParamMap.get('success') === 'true';
    if (justPaid) this.showSuccessBanner.set(true);
    this.fetch(justPaid ? 10 : 0);
  }

  private fetch(retriesLeft: number): void {
    this.service
      .getMine()
      .pipe(catchError(() => of(null)))
      .subscribe((sub) => {
        if (sub) this.subscription.set(sub);
        this.loading.set(false);

        if (retriesLeft > 0 && sub?.plan === PlanType.Free) {
          timer(1000)
            .pipe(take(1))
            .subscribe(() => this.fetch(retriesLeft - 1));
        }
      });
  }

  upgrade(): void {
    if (this.processing()) return;
    this.processing.set(true);
    this.errorMessage.set(null);
    this.service.createCheckout().subscribe({
      next: ({ url }) => (window.location.href = url),
      error: () => {
        this.errorMessage.set('Impossible de démarrer le paiement.');
        this.processing.set(false);
        this.toastService.error('Impossible de démarrer le paiement.');
      },
    });
  }

  manage(): void {
    if (this.processing()) return;
    this.processing.set(true);
    this.errorMessage.set(null);
    this.service.createPortal().subscribe({
      next: ({ url }) => (window.location.href = url),
      error: () => {
        this.errorMessage.set("Impossible d'ouvrir le portail de gestion.");
        this.processing.set(false);
        this.toastService.error("Impossible d'ouvrir le portail de gestion.");
      },
    });
  }
}
