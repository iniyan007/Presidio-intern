import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { signal } from '@angular/core';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5082/api/Auth';
  private http = inject(HttpClient);
  private userService = inject(UserService);

  isAuthenticated = signal<boolean>(false);

  constructor() {
    this.isAuthenticated.set(!!this.getToken());
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data).pipe(
      tap((res: any) => {
        if (res.token) {
          localStorage.setItem('jwt_token', res.token);
          if (res.refreshToken) {
            localStorage.setItem('refresh_token', res.refreshToken);
          }
          this.isAuthenticated.set(true);
          this.userService.loadProfile().subscribe();
        }
      })
    );
  }

  refreshToken(token: string, refreshToken: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/refresh-token`, { token, refreshToken }).pipe(
      tap((res: any) => {
        if (res.token) {
          localStorage.setItem('jwt_token', res.token);
          if (res.refreshToken) {
            localStorage.setItem('refresh_token', res.refreshToken);
          }
          this.isAuthenticated.set(true);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
    this.isAuthenticated.set(false);
    this.userService.userProfile.set(null);
  }

  getToken() {
    return localStorage.getItem('jwt_token');
  }
  
  getRefreshToken() {
    return localStorage.getItem('refresh_token');
  }
}
