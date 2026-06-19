import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;
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

  sendOtp(): Observable<any> {
    const email = this.getEmailFromToken();
    return this.http.post(`${this.apiUrl}/send-otp`, { email });
  }

  verifyOtp(otp: string): Observable<any> {
    const email = this.getEmailFromToken();
    return this.http.post(`${this.apiUrl}/verify-otp`, { email, otp }).pipe(
      tap((res: any) => {
        if (res.authResponse && res.authResponse.token) {
          localStorage.setItem('jwt_token', res.authResponse.token);
          if (res.authResponse.refreshToken) {
            localStorage.setItem('refresh_token', res.authResponse.refreshToken);
          }
          this.isAuthenticated.set(true);
        }
        this.userService.loadProfile().subscribe();
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

  isEmailVerified(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.EmailVerified === 'True' || payload.EmailVerified === true;
    } catch {
      return false;
    }
  }

  getEmailFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || null;
    } catch {
      return null;
    }
  }
}
