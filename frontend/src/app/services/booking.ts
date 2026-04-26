import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = 'http://localhost:5130/api/booking';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return {
      headers: { 'Authorization': `Bearer ${token}` }
    };
  }

  lockSeats(tripId: number, seatNumbers: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/lock`, { tripId, seatNumbers }, this.getHeaders());
  }

  confirmBooking(bookingId: number, passengers: any[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/${bookingId}/confirm`, { passengers }, this.getHeaders());
  }

  getMyBookings(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my-bookings`, this.getHeaders());
  }
}
