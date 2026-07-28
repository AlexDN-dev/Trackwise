import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FreeStatsResponse, PremiumStatsResponse } from '../models/stats.models';

@Injectable({ providedIn: 'root' })
export class StatsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/stats`;

  getFree(): Observable<FreeStatsResponse> {
    return this.http.get<FreeStatsResponse>(`${this.baseUrl}/free`);
  }

  getPremium(): Observable<PremiumStatsResponse> {
    return this.http.get<PremiumStatsResponse>(`${this.baseUrl}/premium`);
  }
}
