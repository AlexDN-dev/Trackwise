import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, firstValueFrom, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterRequest, LoginRequest, AuthResponse } from '../models/auth.models';
import { AiQuotaService } from './ai-quota.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly quotaService = inject(AiQuotaService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly accessToken = signal<string | null>(null);
  readonly isAuthenticated = computed(() => this.accessToken() !== null);
  readonly isAdmin = computed(() => this.roleFromToken(this.accessToken()) === 'Admin');

  private roleFromToken(token: string | null): string | null {
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      let payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const pad = payload.length % 4;
      if (pad) payload += '='.repeat(4 - pad);
      const claims = JSON.parse(atob(payload));
      return claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ?? claims['role']
        ?? null;
    } catch {
      return null;
    }
  }

  private refreshInFlight: Observable<AuthResponse> | null = null;

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request, { withCredentials: true })
      .pipe(
        tap((res) => {
          this.accessToken.set(res.accessToken);
          this.quotaService.refresh().subscribe({ error: () => {} });
        }),
      );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, null, { withCredentials: true }).pipe(
      catchError(() => of(void 0)),
      finalize(() => this.clearLocal()),
    );
  }

  refresh(): Observable<AuthResponse> {
    if (this.refreshInFlight) return this.refreshInFlight;

    this.refreshInFlight = this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, null, { withCredentials: true })
      .pipe(
        tap((res) => this.accessToken.set(res.accessToken)),
        finalize(() => (this.refreshInFlight = null)),
        shareReplay({ bufferSize: 1, refCount: true }),
      );

    return this.refreshInFlight;
  }

  async tryRestoreSession(): Promise<void> {
    try {
      await firstValueFrom(this.refresh());
      this.quotaService.refresh().subscribe({ error: () => {} });
    } catch {
      this.accessToken.set(null);
    }
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  private clearLocal(): void {
    this.accessToken.set(null);
    this.refreshInFlight = null;
    this.quotaService.clear();
  }
}
