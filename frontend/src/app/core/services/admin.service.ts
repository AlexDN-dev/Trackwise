import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminAuditLog,
  AdminStatsResponse,
  AdminUserDetail,
  AdminUsersPage,
  BannedIp,
  LogsResponse,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  listUsers(page: number, size: number, search?: string): Observable<AdminUsersPage> {
    let params = new HttpParams().set('page', page).set('size', size);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<AdminUsersPage>(`${this.baseUrl}/users`, { params });
  }

  getUser(id: string): Observable<AdminUserDetail> {
    return this.http.get<AdminUserDetail>(`${this.baseUrl}/users/${id}`);
  }

  banUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/ban`, null);
  }

  unbanUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/unban`, null);
  }

  grantTrial(id: string, days: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/grant-trial`, { days });
  }

  grantTokens(id: string, count: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/grant-tokens`, { count });
  }

  listIps(): Observable<BannedIp[]> {
    return this.http.get<BannedIp[]>(`${this.baseUrl}/ips`);
  }

  banIp(ip: string, reason: string | null): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/ips/ban`, { ip, reason });
  }

  unbanIp(ip: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/ips/unban`, { ip });
  }

  stats(): Observable<AdminStatsResponse> {
    return this.http.get<AdminStatsResponse>(`${this.baseUrl}/stats`);
  }

  audit(limit = 100): Observable<AdminAuditLog[]> {
    const params = new HttpParams().set('limit', limit);
    return this.http.get<AdminAuditLog[]>(`${this.baseUrl}/audit`, { params });
  }

  logs(lines = 200, level?: string, search?: string): Observable<LogsResponse> {
    let params = new HttpParams().set('lines', lines);
    if (level) params = params.set('level', level);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<LogsResponse>(`${this.baseUrl}/logs`, { params });
  }
}
