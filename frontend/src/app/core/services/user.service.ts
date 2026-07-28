import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { UpdateUserRequest, UserResponse } from '../models/user.models';
import { AuthResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users/me`;

  getMe(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}`);
  }

  update(request: UpdateUserRequest): Observable<UserResponse> {
    return this.http.patch<UserResponse>(this.baseUrl, request);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/users/me/change-password`, {
      currentPassword,
      newPassword,
    });
  }
}
