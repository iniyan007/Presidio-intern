import { ApplicationConfig, provideZonelessChangeDetection, Injector } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient, withInterceptors, HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

import { routes } from './app.routes';

const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const injector = inject(Injector);
  
  const token = localStorage.getItem('jwt_token');
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Global 401 Unauthorized handler
      if (error.status === 401 && !req.url.includes('/api/Auth/login')) {
        const authService = injector.get(AuthService);
        authService.logout();
        // Redirect to login page
        router.navigate(['/auth']);
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
