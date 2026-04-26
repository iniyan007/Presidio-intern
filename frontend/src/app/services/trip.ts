import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private apiUrl = 'http://localhost:5130/api/trip';

  constructor(private http: HttpClient) {}

  searchTrips(source?: string, destination?: string, date?: string): Observable<any[]> {
    let params = new HttpParams();
    if (source) params = params.set('source', source);
    if (destination) params = params.set('destination', destination);
    if (date) params = params.set('date', date);

    return this.http.get<any[]>(`${this.apiUrl}/search`, { params });
  }

  getTripDetails(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }
}
