import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CheckoutSessionResponse, SubscriptionResponse } from '../models/subscription.models';

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/subscription`;

  getMine(): Observable<SubscriptionResponse> {
    return this.http.get<SubscriptionResponse>(`${this.baseUrl}/me`);
  }

  createCheckout(): Observable<CheckoutSessionResponse> {
    return this.http.post<CheckoutSessionResponse>(`${this.baseUrl}/checkout`, {});
  }

  createPortal(): Observable<CheckoutSessionResponse> {
    return this.http.post<CheckoutSessionResponse>(`${this.baseUrl}/portal`, {});
  }
}
