import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { PlanType } from '../../core/models/user.models';
import { AiQuotaBadge } from '../../shared/components/ai-quota-badge/ai-quota-badge';

@Component({
  selector: 'app-profile',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AiQuotaBadge],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly subscriptionService = inject(SubscriptionService);

  readonly plan = signal<PlanType | null>(null);
  readonly isPremium = computed(() => this.plan() === PlanType.Premium);
  readonly planLabel = computed(() => (this.isPremium() ? 'Premium' : 'Free'));

  readonly mobileMenuOpen = signal(false);
  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }
  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  ngOnInit(): void {
    this.subscriptionService.getMine().subscribe({
      next: (sub) => this.plan.set(sub.plan),
    });
  }
  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
