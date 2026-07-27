import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { AddUserCompetenceRequest, CompetenceResponse } from '../models/competence.models';

@Injectable({ providedIn: 'root' })
export class CompetenceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getMine(): Observable<CompetenceResponse[]> {
    return this.http.get<CompetenceResponse[]>(`${this.apiUrl}/users/me/competences`);
  }

  add(request: AddUserCompetenceRequest): Observable<CompetenceResponse> {
    return this.http.post<CompetenceResponse>(`${this.apiUrl}/users/me/competences`, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/me/competences/${id}`);
  }

  search(term: string, limit = 10): Observable<CompetenceResponse[]> {
    const params = new HttpParams().set('search', term).set('limit', limit.toString());
    return this.http.get<CompetenceResponse[]>(`${this.apiUrl}/competences`, { params });
  }
}
