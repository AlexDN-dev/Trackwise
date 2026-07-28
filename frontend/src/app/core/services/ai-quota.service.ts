import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AiQuotaStatus } from '../models/ai-quota.models';
import { PlanType } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class AiQuotaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users/me/ai-quota`;

  readonly quota = signal<AiQuotaStatus | null>(null);

  readonly isPremium = computed(() => this.quota()?.plan === PlanType.Premium);
  readonly ratio = computed(() => {
    const q = this.quota();
    if (!q || q.limit === 0) return 0;
    return Math.min(1, q.used / q.limit);
  });

  refresh(): Observable<AiQuotaStatus> {
    return this.http
      .get<AiQuotaStatus>(this.baseUrl)
      .pipe(tap((q) => this.quota.set(q)));
  }

  clear(): void {
    this.quota.set(null);
  }
}
