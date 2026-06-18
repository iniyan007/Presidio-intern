import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

import { UserProfile, UpdateProfileRequest } from '../models/user.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Users`;

  userProfile = signal<UserProfile | null>(null);

  constructor() {
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
