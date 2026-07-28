import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AiJob, AnalysisResult, AnalyzeRequest } from '../models/ai.models';
import { AiQuotaService } from './ai-quota.service';

@Injectable({ providedIn: 'root' })
export class AiAnalysisService {
  private readonly http = inject(HttpClient);
  private readonly quotaService = inject(AiQuotaService);
  private readonly baseUrl = `${environment.apiUrl}/candidatures/analyze`;
  private readonly _openRequest = signal<string | null>(null);

  private readonly _jobs = signal<AiJob[]>([]);

  readonly openRequest = this._openRequest.asReadonly();
  readonly activeJob = computed(() => this._jobs().find((j) => j.status === 'running') ?? null);

  readonly readyJob = computed(() => this._jobs().find((j) => j.status === 'succeeded') ?? null);

  readonly failedJob = computed(() => this._jobs().find((j) => j.status === 'failed') ?? null);

  analyze(text: string): void {
    const id = crypto.randomUUID();
    const job: AiJob = { id, status: 'running', startedAt: new Date() };
    this._jobs.update((list) => [...list, job]);

    const body: AnalyzeRequest = { text };
    this.http.post<AnalysisResult>(this.baseUrl, body).subscribe({
      next: (result) => {
        this.updateJob(id, { status: 'succeeded', result });
        this.quotaService.refresh().subscribe({ error: () => {} });
      },
      error: (err) => {
        const is429 = err?.status === 429;
        if (is429 && err?.error?.quota) {
          this.quotaService.quota.set(err.error.quota);
        } else {
          this.quotaService.refresh().subscribe({ error: () => {} });
        }
        this.updateJob(id, {
          status: 'failed',
          error: is429
            ? 'Quota IA atteint. Passe Premium pour en avoir plus.'
            : "L'analyse a échoué. Réessaie ou saisi manuellement.",
        });
      },
    });
  }

  consume(id: string): void {
    this._jobs.update((list) => list.filter((j) => j.id !== id));
  }

  requestOpen(jobId: string): void {
    this._openRequest.set(jobId);
  }

  clearOpenRequest(): void {
    this._openRequest.set(null);
  }

  private updateJob(id: string, patch: Partial<AiJob>): void {
    this._jobs.update((list) => list.map((j) => (j.id === id ? { ...j, ...patch } : j)));
  }
}
