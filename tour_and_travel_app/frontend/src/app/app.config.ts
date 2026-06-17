import { ApplicationConfig, provideZonelessChangeDetection, Injector } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient, withInterceptors, HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { throwError, BehaviorSubject, Observable } from 'rxjs';

import { routes } from './app.routes';

let isRefreshing = false;
let refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

const addToken = (req: any, token: string) => {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
};

const handle401Error = (req: any, next: any, injector: Injector, error: HttpErrorResponse): Observable<any> => {
  const authService = injector.get(AuthService);
  const router = injector.get(Router);

  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const token = localStorage.getItem('jwt_token');
    const refreshToken = localStorage.getItem('refresh_token');

    if (token && refreshToken) {
      return authService.refreshToken(token, refreshToken).pipe(
        switchMap((res: any) => {
          isRefreshing = false;
          refreshTokenSubject.next(res.token);
          return next(addToken(req, res.token));
        }),
        catchError((err) => {
          isRefreshing = false;
          authService.logout();
          router.navigate(['/auth']);
          return throwError(() => err);
        })
      );
    } else {
      isRefreshing = false;
      authService.logout();
      router.navigate(['/auth']);
      return throwError(() => error);
    }
  } else {
    return refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(jwt => {
        return next(addToken(req, jwt));
      })
    );
  }
};

const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const injector = inject(Injector);
  
  const token = localStorage.getItem('jwt_token');
  if (token) {
    req = addToken(req, token);
  }
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized globally
      if (error.status === 401 && !req.url.includes('/api/Auth/login') && !req.url.includes('/api/Auth/refresh-token')) {
        return handle401Error(req, next, injector, error);
      }
      return throwError(() => error);
    })
  );
};

const idempotencyInterceptor: HttpInterceptorFn = (req, next) => {
  // Only apply to state-mutating requests
  if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method)) {
    // Generate a UUID (or fallback random string)
    const idempotencyKey = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2) + Date.now().toString(36);
    
    // Do not overwrite if already set
    if (!req.headers.has('X-Idempotency-Key')) {
      req = req.clone({
        setHeaders: {
          'X-Idempotency-Key': idempotencyKey
        }
      });
    }
  }
  return next(req);
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, idempotencyInterceptor]))
  ]
};
