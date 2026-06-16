import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TravelPackage {
  id: string;
  packagerName: string;
  title: string;
  packageType: string;
  destination: string;
  country: string;
  durationDays: number;
  durationNights: number;
  avgRating: number;
  totalReviews: number;
  primaryImageUrl: string | null;
  startingPrice: number;
  pendingSeats: number;
  earliestDepartureDate: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class PackageService {
  private apiUrl = 'http://localhost:5082/api/Packages';
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
    }
    return this.http.get<any>(this.apiUrl, { params });
  }

  getPackageById(id: string): Observable<TravelPackage> {
    return this.http.get<TravelPackage>(`${this.apiUrl}/${id}`);
  }
}
