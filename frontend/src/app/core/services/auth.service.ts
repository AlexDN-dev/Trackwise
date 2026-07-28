import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, firstValueFrom, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterRequest, LoginRequest, AuthResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly accessToken = signal<string | null>(null);
  readonly isAuthenticated = computed(() => this.accessToken() !== null);

  private refreshInFlight: Observable<AuthResponse> | null = null;

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request, { withCredentials: true })
      .pipe(tap((res) => this.accessToken.set(res.accessToken)));
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
  }
}
