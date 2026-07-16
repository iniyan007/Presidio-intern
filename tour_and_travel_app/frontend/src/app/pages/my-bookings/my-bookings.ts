import { HttpClient } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../services/booking.service';
import { BookingResponse } from '../../models/booking.model';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css'
})
export class MyBookingsComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  environment = environment;
  bookingService = inject(BookingService);
  packageService = inject(PackageService);
  toastService = inject(ToastService);
  private http = inject(HttpClient);

  bookings = signal<BookingResponse[]>([]);
  packageTitles = signal<Record<string, string>>({});
  expandedBookings = signal<Set<string>>(new Set<string>());

  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // Cancellation State
  isCancelling = signal<string | null>(null);
  cancellationReason = signal<string>('');
  isPolicyAware = signal<boolean>(false);
  
  // Document Reupload State
  isUploading = signal<string | null>(null); // Document ID

  ngOnInit() {
    this.fetchBookings();
  }

  fetchBookings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.bookingService.getMyBookings().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.bookings.set(res);
        this.fetchPackageTitles(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Failed to load your bookings.');
        this.isLoading.set(false);
      }
    });
  }

  fetchPackageTitles(bookingsList: BookingResponse[]) {
    const uniqueIds = Array.from(new Set(bookingsList.map(b => b.packageId)));
    uniqueIds.forEach(id => {
      if (!this.packageTitles()[id]) {
        this.packageService.getPackageById(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: (pkg) => {
            this.packageTitles.update(t => ({ ...t, [id]: pkg.title }));
          }
        });
      }
    });
  }

  toggleBooking(bookingId: string) {
    const expanded = new Set(this.expandedBookings());
    if (expanded.has(bookingId)) {
      expanded.delete(bookingId);
    } else {
      expanded.add(bookingId);
    }
    this.expandedBookings.set(expanded);
  }

  isExpanded(bookingId: string): boolean {
    return this.expandedBookings().has(bookingId);
  }

  downloadTicket(bookingId: string) {
    this.bookingService.downloadTicket(bookingId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Ticket-${bookingId}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: () => this.toastService.show('Failed to download ticket.', 'error')
    });
  }

  initiateCancel(bookingId: string) {
    this.isCancelling.set(bookingId);
    this.cancellationReason.set('');
    this.isPolicyAware.set(false);
  }

  cancelBooking(bookingId: string) {
    if (!this.cancellationReason().trim()) {
      this.toastService.show('Please provide a reason for cancellation.', 'error');
      return;
    }

    if (!this.isPolicyAware()) {
      this.toastService.show('Please confirm you are aware of the cancellation and refund policy.', 'error');
      return;
    }

    this.bookingService.cancelBooking(bookingId, { cancellationReason: this.cancellationReason() }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show('Booking cancelled successfully.', 'success');
        this.isCancelling.set(null);
        this.fetchBookings();
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Failed to cancel booking.', 'error');
      }
    });
  }

  onFileSelected(event: any, documentId: string) {
    const file = event.target.files[0];
    if (!file) return;

    this.isUploading.set(documentId);
    this.bookingService.reuploadDocument(documentId, file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show('Document re-uploaded successfully.', 'success');
        this.isUploading.set(null);
        this.fetchBookings(); // Refresh statuses
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Failed to upload document.', 'error');
        this.isUploading.set(null);
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Verified': return 'bg-emerald-100 text-emerald-800 border-emerald-200';
      case 'Rejected': return 'bg-error/10 text-error border-error/20';
      case 'Pending': return 'bg-amber-100 text-amber-800 border-amber-200';
      default: return 'bg-surface-container-high text-on-surface-variant';
    }
  }

  getBookingStatusBadge(status: string): string {
    switch (status) {
      case 'Confirmed': return 'bg-emerald-100 text-emerald-800 border-emerald-200';
      case 'Pending': return 'bg-amber-100 text-amber-800 border-amber-200';
      case 'DocumentUnderReview': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'Cancelled': return 'bg-error/10 text-error border-error/20';
      case 'Completed': return 'bg-purple-100 text-purple-800 border-purple-200';
      case 'Refunded': return 'bg-gray-100 text-gray-800 border-gray-200';
      default: return 'bg-surface-container-high text-on-surface-variant';
    }
  }

  getPaymentStatusBadge(status: string): string {
    if (status === 'Paid') return 'bg-emerald-100 text-emerald-800';
    if (status === 'Partial') return 'bg-blue-100 text-blue-800';
    return 'bg-error/10 text-error';
  }

  downloadDocument(fileUrl: string) {
    if (!fileUrl) return;
    const fullUrl = fileUrl.startsWith('http') ? fileUrl : `${environment.baseUrl}${fileUrl}`;
    if (!fullUrl.includes('/api/documents/proxy')) {
       window.open(fullUrl, '_blank');
       return;
    }
    this.toastService.show('Opening document...', 'success');
    this.http.get(fullUrl, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 10000);
      },
      error: () => this.toastService.show('Failed to fetch secure document.', 'error')
    });
  }
}
