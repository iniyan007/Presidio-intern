import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  profilePicture: string | null;
  isActive: boolean;
  isEmailVerified: boolean;
  isPackager: boolean;
}

export interface UpdateProfileRequest {
  fullName: string;
  phone?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `http://localhost:5082/api/Users`;

  userProfile = signal<UserProfile | null>(null);

  constructor() {
    // If token exists, load profile on startup
    if (localStorage.getItem('jwt_token')) {
      this.loadProfile().subscribe({
        error: () => console.log('Failed to load profile on startup')
      });
    }
  }

  loadProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`).pipe(
      tap((profile: UserProfile) => this.userProfile.set(profile))
    );
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, request);
  }

  uploadProfilePicture(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('profilePicture', file);
    return this.http.post(`${this.apiUrl}/profile/picture`, formData);
  }

  removeProfilePicture(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/profile/picture`);
  }
}
