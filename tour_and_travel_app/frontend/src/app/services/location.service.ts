import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface LocationSuggestion {
  display_name: string;
  name: string;
  address: {
    city?: string;
    town?: string;
    village?: string;
    state?: string;
    country?: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private http = inject(HttpClient);
  private apiUrl = 'https://nominatim.openstreetmap.org/search';

  searchLocation(query: string): Observable<LocationSuggestion[]> {
    if (!query || query.trim() === '') {
      return of([]);
    }
    const params = new HttpParams()
      .set('q', query)
      .set('format', 'json')
      .set('addressdetails', '1')
      .set('limit', '5')
      .set('featuretype', 'settlement');

    return this.http.get<any[]>(this.apiUrl, { params }).pipe(
      map(results => {
        return results.map(item => ({
          display_name: item.display_name,
          name: item.name,
          address: item.address || {}
        }));
      }),
      catchError(error => {
        console.error('Error fetching location suggestions', error);
        return of([]);
      })
    );
  }
}
