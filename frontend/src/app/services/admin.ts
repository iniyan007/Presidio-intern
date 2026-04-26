import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'http://localhost:5130/api/admin';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  getStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/stats`, this.getHeaders());
  }

  getRoutes(): Observable<any> {
    return this.http.get(`${this.apiUrl}/routes`, this.getHeaders());
  }

  createRoute(routeData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/routes`, routeData, this.getHeaders());
  }

  deleteRoute(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/routes/${id}`, this.getHeaders());
  }

  getPendingOperators(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pending-operators`, this.getHeaders());
  }

  getAllOperators(): Observable<any> {
    return this.http.get(`${this.apiUrl}/operators`, this.getHeaders());
  }

  approveOperator(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/operators/${id}/approve`, {}, this.getHeaders());
  }

  rejectOperator(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/operators/${id}/reject`, this.getHeaders());
  }

  getPendingBuses(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pending-buses`, this.getHeaders());
  }

  getAllBuses(): Observable<any> {
    return this.http.get(`${this.apiUrl}/buses`, this.getHeaders());
  }

  approveBus(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/buses/${id}/approve`, {}, this.getHeaders());
  }

  rejectBus(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/buses/${id}/reject`, this.getHeaders());
  }
}
