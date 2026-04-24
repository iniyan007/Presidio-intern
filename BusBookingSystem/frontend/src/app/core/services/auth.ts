import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { tap, catchError, throwError } from 'rxjs';

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  phone?: string;
  age?: number;
  gender?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private currentUserSignal = signal<User | null>(null);
  
  currentUser = computed(() => this.currentUserSignal());
  isAuthenticated = computed(() => !!this.currentUserSignal());
  isOperator = computed(() => this.currentUserSignal()?.role === 'OPERATOR');
  isAdmin = computed(() => this.currentUserSignal()?.role === 'ADMIN');

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage() {
    if (typeof window !== 'undefined') {
      const storedUser = localStorage.getItem('user');
      if (storedUser) {
        this.currentUserSignal.set(JSON.parse(storedUser));
      }
    }
  }

  login(credentials: any) {
    return this.http.post<any>(`${environment.apiUrl}/Auth/login`, credentials).pipe(
      tap(res => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', res.token);
          localStorage.setItem('user', JSON.stringify(res.user));
          this.currentUserSignal.set(res.user);
        }
      })
    );
  }

  register(data: any) {
    return this.http.post<any>(`${environment.apiUrl}/Auth/register`, data);
  }

  logout() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      this.currentUserSignal.set(null);
      this.router.navigate(['/login']);
    }
  }

  updateProfile(data: any) {
    return this.http.put<any>(`${environment.apiUrl}/Auth/profile`, data).pipe(
      tap(() => {
        // Optimistically update or fetch again
        const current = this.currentUserSignal();
        if (current) {
          const updated = { ...current, ...data };
          if (typeof window !== 'undefined') {
            localStorage.setItem('user', JSON.stringify(updated));
          }
          this.currentUserSignal.set(updated);
        }
      })
    );
  }
}
