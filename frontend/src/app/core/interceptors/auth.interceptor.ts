import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // on ne touche pas aux endpoints publics d'auth
  const isAuthCall = /\/auth\/(login|register|refresh)/.test(req.url);
  const token = auth.getAccessToken();

  const authReq =
    token && !isAuthCall ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // 401 sur une requête protégée → on tente un refresh puis on rejoue
      if (err.status === 401 && !isAuthCall) {
        return auth.refresh().pipe(
          switchMap((res) =>
            next(req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } })),
          ),
          catchError((refreshErr) => {
            auth.logout().subscribe();
            router.navigate(['/auth']);
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};
