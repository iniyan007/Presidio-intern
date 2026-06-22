import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/Admin`;
  private http = inject(HttpClient);

  getPendingPackagers(search?: string, sort?: string): Observable<any> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (sort) params = params.set('sort', sort);
    return this.http.get(`${this.apiUrl}/packagers/pending`, { params });
  }

  getApprovedPackagers(search?: string, sort?: string): Observable<any> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (sort) params = params.set('sort', sort);
    return this.http.get(`${this.apiUrl}/packagers/approved`, { params });
  }

  getDeactivatedPackagers(search?: string, sort?: string): Observable<any> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (sort) params = params.set('sort', sort);
    return this.http.get(`${this.apiUrl}/packagers/deactivated`, { params });
  }

  getPackagerDocuments(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/packagers/${id}/documents`);
  }

  approvePackager(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/packagers/${id}/approve`, {});
  }

  rejectPackager(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/packagers/${id}/reject`, { reason });
  }

  deactivatePackager(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/packagers/${id}/deactivate`, { reason });
  }

  activatePackager(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/packagers/${id}/activate`, {});
  }

  getAnalytics(): Observable<any> {
    return this.http.get(`${this.apiUrl}/analytics`);
  }

  getPlatformConfig(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/PlatformConfig`);
  }

  updatePlatformConfig(platformFeePercent: number, gstPercent: number): Observable<any> {
    return this.http.put(`${environment.apiUrl}/PlatformConfig`, { platformFeePercent, gstPercent });
  }
}
