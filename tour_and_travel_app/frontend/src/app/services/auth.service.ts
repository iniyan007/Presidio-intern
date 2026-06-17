import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { signal } from '@angular/core';
import { UserService } from './user';

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
          this.isAuthenticated.set(true);
          this.userService.loadProfile().subscribe();
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    this.isAuthenticated.set(false);
    this.userService.userProfile.set(null);
  }

  getToken() {
    return localStorage.getItem('jwt_token');
  }
}
