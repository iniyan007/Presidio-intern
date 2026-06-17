import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TravelDocumentResponse {
  id: string;
  documentType: string;
  filePath: string;
  fileName: string;
  uploadedAt: string;
  status: string;
  rejectionReason: string | null;
}

export interface BookingTravelerResponse {
  id: string;
  fullName: string;
  passportNumber: string | null;
  dateOfBirth: string | null;
  nationality: string | null;
  age: number | null;
  gender: string | null;
  mealPreference: string | null;
  aadharCardNumber: string | null;
  isPrimary: boolean;
  documents: TravelDocumentResponse[];
}

export interface BookingResponse {
  id: string;
  userId: string;
  packageId: string;
  bookingReference: string;
  adultCount: number;
  childCount: number;
  infantCount: number;
  totalAmount: number;
  paidAmount: number;
  status: string;
  paymentStatus: string;
  travelDate: string;
  returnDate: string;
  specialRequests: string | null;
  bookedAt: string;
  cancelledAt: string | null;
  cancellationReason: string | null;
  travelers: BookingTravelerResponse[];
}

export interface CancelBookingRequest {
  reason: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5082/api/Bookings';

  getMyBookings(): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(`${this.apiUrl}/my-bookings`);
  }

  cancelBooking(id: string, request: CancelBookingRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/cancel`, request);
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
