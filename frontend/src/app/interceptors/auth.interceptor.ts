import { HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const auth = inject(AuthService);
  const access = auth.getAccessToken();
  let authReq = req;
  if (access) {
    authReq = req.clone({ setHeaders: { Authorization: `Bearer ${access}` } });
  }

  return next(authReq).pipe(
    catchError(err => {
      if (err.status === 401) {
        return auth.refresh().pipe(
          switchMap(r => {
            if (r?.accessToken) {
              localStorage.setItem('sg.accessToken', r.accessToken);
              const retried = req.clone({ setHeaders: { Authorization: `Bearer ${r.accessToken}` } });
              return next(retried);
            }
            return throwError(() => err);
          })
        );
      }
      return throwError(() => err);
    })
  );
};


