import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../services/booking.service';
import { BookingResponse } from '../../models/booking.model';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css'
})
export class MyBookingsComponent implements OnInit {
  bookingService = inject(BookingService);
  packageService = inject(PackageService);
  toastService = inject(ToastService);

  bookings = signal<BookingResponse[]>([]);
  packageTitles = signal<Record<string, string>>({});
  expandedBookings = signal<Set<string>>(new Set<string>());

  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // Cancellation State
  isCancelling = signal<string | null>(null);
  cancellationReason = signal<string>('');
  
  // Document Reupload State
  isUploading = signal<string | null>(null); // Document ID

  ngOnInit() {
    this.fetchBookings();
  }

  fetchBookings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.bookingService.getMyBookings().subscribe({
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
        this.packageService.getPackageById(id).subscribe({
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
    this.bookingService.downloadTicket(bookingId).subscribe({
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
  }

  cancelBooking(bookingId: string) {
    if (!this.cancellationReason().trim()) {
      this.toastService.show('Please provide a reason for cancellation.', 'error');
      return;
    }

    if (!confirm('Are you sure you want to cancel this booking? This action cannot be undone.')) return;

    this.bookingService.cancelBooking(bookingId, { reason: this.cancellationReason() }).subscribe({
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
    this.bookingService.reuploadDocument(documentId, file).subscribe({
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
}
