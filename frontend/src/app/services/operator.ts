import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OperatorService {
  private apiUrl = 'http://localhost:5130/api/operator';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  getRevenueStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/revenue`, this.getHeaders());
  }

  getRoutes(): Observable<any[]> {
    return this.http.get<any[]>('http://localhost:5130/api/route', this.getHeaders());
  }

  getMyBuses(): Observable<any> {
    return this.http.get(`${this.apiUrl}/buses`, this.getHeaders());
  }

  addBus(busData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/buses`, busData, this.getHeaders());
  }

  getMyTrips(): Observable<any> {
    return this.http.get(`${this.apiUrl}/trips`, this.getHeaders());
  }

  createTrip(tripData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/trips`, tripData, this.getHeaders());
  }

  deleteTrip(tripId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/trips/${tripId}`, this.getHeaders());
  }

  getTripBookings(tripId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/trips/${tripId}/passengers`, this.getHeaders());
  }
}
