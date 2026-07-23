import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import {
  CandidatureResponse,
  CreateCandidatureRequest,
  UpdateCandidatureRequest,
} from '../models/candidature.models';

@Injectable({providedIn: 'root'})
export class CandidatureService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/candidatures`;

  getAll(): Observable<CandidatureResponse[]> {
    return this.http.get<CandidatureResponse[]>(this.baseUrl);
  }

  create(request: CreateCandidatureRequest): Observable<CandidatureResponse> {
    return this.http.post<CandidatureResponse>(this.baseUrl, request);
  }

  update(id: string, request: UpdateCandidatureRequest): Observable<CandidatureResponse> {
    return this.http.put<CandidatureResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string, request: UpdateCandidatureRequest): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
