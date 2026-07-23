import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { EntrepriseResponse } from '../models/candidature.models';

@Injectable({providedIn: 'root'})
export class EntrepriseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/entreprises`;

  search(term: string): Observable<EntrepriseResponse[]> {
    const params = new HttpParams().set('search', term);
    return this.http.get<EntrepriseResponse[]>(this.baseUrl, {params});
  }
}
