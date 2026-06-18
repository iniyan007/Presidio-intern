import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BookingResponse, CancelBookingRequest, PlatformConfigResponse, ProcessPaymentRequest } from '../models/booking.model';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5082/api/Bookings';
  private platformUrl = 'http://localhost:5082/api/PlatformConfig';

  getPlatformConfig(): Observable<PlatformConfigResponse> {
    return this.http.get<PlatformConfigResponse>(this.platformUrl);
  }

  createBooking(formData: FormData): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.apiUrl, formData);
  }

  getMyBookings(): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(`${this.apiUrl}/my-bookings`);
  }

  cancelBooking(id: string, request: CancelBookingRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/cancel`, request);
  }

  processPayment(id: string, request: ProcessPaymentRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/pay`, request);
  }

  downloadTicket(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/ticket`, { responseType: 'blob' });
  }

  reuploadDocument(documentId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.put(`${this.apiUrl}/documents/${documentId}/reupload`, formData);
  }

  createReview(bookingId: string, reviewData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${bookingId}/reviews`, reviewData);
  }

  uploadReviewMedia(files: File[]): Observable<{ success: boolean, paths: string[] }> {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });
    // The endpoint is actually on the base API URL, not under /Bookings
    // so we need to construct it carefully.
    const baseUrl = this.apiUrl.replace('/api/Bookings', '');
    return this.http.post<{ success: boolean, paths: string[] }>(`${baseUrl}/api/Reviews/upload-media`, formData);
  }
}
