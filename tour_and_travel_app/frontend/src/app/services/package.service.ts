import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { TravelPackage, TravelPackageDetails, PackageReview } from '../models/package.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PackageService {
  private apiUrl = `${environment.apiUrl}/Packages`;
  private http = inject(HttpClient);

  constructor() { }

  getPackages(filters?: any): Observable<any> {
    let params = new HttpParams();
    if (filters) {
      if (filters.SearchTerm) params = params.set('SearchTerm', filters.SearchTerm);
      if (filters.Destination) params = params.set('Destination', filters.Destination);
      if (filters.MinPrice) params = params.set('MinPrice', filters.MinPrice);
      if (filters.MaxPrice) params = params.set('MaxPrice', filters.MaxPrice);
      if (filters.PackageType) params = params.set('PackageType', filters.PackageType);
      if (filters.TravelStartDate) params = params.set('TravelStartDate', filters.TravelStartDate);
      if (filters.SortBy) params = params.set('SortBy', filters.SortBy);
      if (filters.PackagerName) params = params.set('PackagerName', filters.PackagerName);
      if (filters.PageNumber) params = params.set('PageNumber', filters.PageNumber);
      if (filters.PageSize) params = params.set('PageSize', filters.PageSize);
    }
    return this.http.get<any>(this.apiUrl, { params });
  }

  getPackageById(id: string): Observable<TravelPackageDetails> {
    return this.http.get<TravelPackageDetails>(`${this.apiUrl}/${id}`);
  }

  getPackageReviews(packageId: string): Observable<PackageReview[]> {
    return this.http.get<PackageReview[]>(`${this.apiUrl}/${packageId}/reviews`);
  }

  createPackage(formData: FormData): Observable<any> {
    return this.http.post<any>(this.apiUrl, formData);
  }

  getMyPackages(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/me`);
  }

  getMyPackageById(id: string): Observable<TravelPackageDetails> {
    return this.http.get<TravelPackageDetails>(`${this.apiUrl}/me/${id}`);
  }

  updateFullPackage(id: string, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/me/${id}`, formData);
  }

  getItineraryChecklist(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/itinerary-checklist`);
  }
}
