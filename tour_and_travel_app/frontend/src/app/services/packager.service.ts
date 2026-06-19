import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PublicPackagerResponse, PackagerReviewResponse } from '../models/packager.model';

@Injectable({
  providedIn: 'root'
})
export class PackagerService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Packagers`;

  searchPublicPackagers(searchTerm: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/public?SearchTerm=${searchTerm}`);
  }

  getPublicPackagerByName(name: string): Observable<PublicPackagerResponse> {
    return this.http.get<PublicPackagerResponse>(`${this.apiUrl}/public/${name}`);
  }

  getPackagerReviews(packagerId: string): Observable<PackagerReviewResponse[]> {
    return this.http.get<PackagerReviewResponse[]>(`${environment.apiUrl}/Packagers/${packagerId}/reviews`);
  }

  applyToBecomePackager(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/apply`, formData);
  }

  getMyPackagerStatus(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/me/status`);
  }
}
