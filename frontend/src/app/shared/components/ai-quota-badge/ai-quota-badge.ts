import { Component, OnInit, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AiQuotaService } from '../../../core/services/ai-quota.service';

@Component({
  selector: 'app-ai-quota-badge',
  imports: [RouterLink],
  templateUrl: './ai-quota-badge.html',
  styleUrl: './ai-quota-badge.css',
})
export class AiQuotaBadge implements OnInit {
  private readonly quotaService = inject(AiQuotaService);

  readonly quota = this.quotaService.quota;
  readonly isPremium = this.quotaService.isPremium;
  readonly ratio = this.quotaService.ratio;

  readonly barPercent = computed(() => {
    if (this.isPremium()) return 100;
    return Math.round(this.ratio() * 100);
  });

  readonly isDanger = computed(() => !this.isPremium() && this.ratio() >= 0.8);

  ngOnInit(): void {
    if (!this.quota()) {
      this.quotaService.refresh().subscribe({ error: () => {} });
    }
  }
}
