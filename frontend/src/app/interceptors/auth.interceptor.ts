import { HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const auth = inject(AuthService);
  const isGoogleFlow = req.url.includes('/google/oauth') || req.url.includes('/google/complete-signup');
  const access = isGoogleFlow ? null : auth.getAccessToken();
  let authReq = req;
  if (access) {
    authReq = req.clone({ setHeaders: { Authorization: `Bearer ${access}` } });
  }

  return next(authReq).pipe(
    catchError(err => {
      if (err.status === 401) {
        const refreshToken = auth.getRefreshToken();
        if (!refreshToken) {
          auth.clearTokens();
          return throwError(() => err);
        }

        return auth.refresh().pipe(
          switchMap(r => {
            if (r?.accessToken) {
              localStorage.setItem('sg.accessToken', r.accessToken);
              const retried = req.clone({ setHeaders: { Authorization: `Bearer ${r.accessToken}` } });
              return next(retried);
            }
            auth.clearTokens();
            return throwError(() => err);
          }),
          catchError(refreshErr => {
            auth.clearTokens();
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => err);
    })
  );
};


