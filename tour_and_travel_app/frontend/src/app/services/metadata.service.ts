import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MetadataEnums {
  packageTypes: string[];
  packageStatuses: string[];
  inclusionTypes: string[];
  mediaCategories: string[];
  mealTypes: string[];
  transportModes: string[];
  daySessions: string[];
  bookingStatuses: string[];
  paymentStatuses: string[];
}

@Injectable({
  providedIn: 'root'
})
export class MetadataService {
  private apiUrl = `${environment.apiUrl}/Metadata`;
  private http = inject(HttpClient);

  getEnums(): Observable<MetadataEnums> {
    return this.http.get<MetadataEnums>(`${this.apiUrl}/enums`);
  }

  getCountries(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/countries`);
  }

  getDestinations(country?: string): Observable<string[]> {
    let params = new HttpParams();
    if (country) {
      params = params.set('country', country);
    }
    return this.http.get<string[]>(`${this.apiUrl}/destinations`, { params });
  }
}
