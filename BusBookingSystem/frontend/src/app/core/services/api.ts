import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  // --- PUBLIC ---
  getRoutes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Route`);
  }

  getAllTrips(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Trip`);
  }

  searchTrips(source: string, destination: string, date: string): Observable<any[]> {
    let params = new HttpParams()
      .set('source', source)
      .set('destination', destination);
    if (date) {
      params = params.set('date', date);
    }
    return this.http.get<any[]>(`${this.baseUrl}/Trip/search`, { params });
  }

  getSeats(tripId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Trip/seats`, { params: new HttpParams().set('tripId', tripId.toString()) });
  }

  getAllBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Bus`);
  }

  // --- USER BOOKING ---
  lockSeat(tripId: number, seatIds: number[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/Booking/lock`, { tripId, seatIds }, { responseType: 'text' as 'json' });
  }

  createBooking(tripId: number, seatIds: number[]): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Booking/create`, { tripId, seatIds });
  }

  cancelBooking(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Booking/cancel/${id}`, {}, { responseType: 'text' as 'json' });
  }

  getMyBookings(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Booking/my-bookings`);
  }

  // --- OPERATOR ---
  registerOperator(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Operator/register`, data, { responseType: 'text' as 'json' });
  }

  getOperatorBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Bus/operator`);
  }

  addBus(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Bus`, data, { responseType: 'text' as 'json' });
  }
  
  updateBus(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Bus/${id}`, data, { responseType: 'text' as 'json' });
  }

  deleteBus(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Bus/${id}`, { responseType: 'text' as 'json' });
  }

  createTrip(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Trip`, data, { responseType: 'text' as 'json' });
  }

  updateTrip(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Trip/${id}`, data, { responseType: 'text' as 'json' });
  }

  deleteTrip(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Trip/${id}`, { responseType: 'text' as 'json' });
  }

  getOperatorRevenue(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Booking/revenue`);
  }

  getOperatorBookings(tripId?: number, page: number = 1, pageSize: number = 10): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (tripId) params = params.set('tripId', tripId.toString());
    return this.http.get<any>(`${this.baseUrl}/Booking/operator-bookings`, { params });
  }

  // --- ADMIN ---
  getAllOperators(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Operator`);
  }
  
  getPendingOperators(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Operator/pending`);
  }

  approveOperator(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Operator/approve/${id}`, {}, { responseType: 'text' as 'json' });
  }

  rejectOperator(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Operator/reject/${id}`, {}, { responseType: 'text' as 'json' });
  }

  disableOperator(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Operator/disable/${id}`, {}, { responseType: 'text' as 'json' });
  }

  getPendingBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Bus/pending`);
  }

  approveBus(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Bus/approve/${id}`, {}, { responseType: 'text' as 'json' });
  }

  rejectBus(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Bus/reject/${id}`, {}, { responseType: 'text' as 'json' });
  }

  createRoute(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Route`, data, { responseType: 'text' as 'json' });
  }

  updateRoute(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Route/${id}`, data, { responseType: 'text' as 'json' });
  }

  deleteRoute(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Route/${id}`, { responseType: 'text' as 'json' });
  }

  getTotalRevenue(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Booking/total-revenue`);
  }
}
