import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-bookings',
  imports: [CommonModule],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css'
})
export class BookingsComponent implements OnInit {
  api = inject(ApiService);
  bookings: any[] = [];
  loading = true;

  ngOnInit() {
    this.fetchBookings();
  }

  fetchBookings() {
    this.loading = true;
    this.api.getMyBookings().subscribe({
      next: (res) => {
        this.bookings = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  cancelBooking(id: number) {
    if (confirm('Are you sure you want to cancel this booking? Refund will be initiated.')) {
      this.api.cancelBooking(id).subscribe({
        next: () => {
          alert('Booking cancelled successfully.');
          const b = this.bookings.find(bk => bk.id === id);
          if(b) b.status = 'CANCELLED';
        },
        error: (err) => {
          alert(err.error?.message || 'Failed to cancel booking.');
        }
      });
    }
  }

  canCancel(booking: any): boolean {
    if (booking.status !== 'CONFIRMED') return false;
    
    // Check if 24 hours before departure
    const departureTime = new Date(booking.departureTime).getTime();
    const now = new Date().getTime();
    const diffHours = (departureTime - now) / (1000 * 60 * 60);
    
    return diffHours >= 24;
  }
}
