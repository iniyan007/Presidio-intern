import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { BookingService } from '../../services/booking.service';
import { BookingResponse } from '../../models/booking.model';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './payment.html'
})
export class PaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookingService = inject(BookingService);

  booking = signal<BookingResponse | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  // Dummy payment fields
  paymentMethod = signal<string>('GPay');

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      // Find the booking in user's bookings to get the total amount
      this.bookingService.getMyBookings().subscribe({
        next: (bookings) => {
          const found = bookings.find(b => b.id === id);
          if (found) {
            this.booking.set(found);
          } else {
            this.errorMessage.set('Booking not found or you do not have permission to view it.');
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.errorMessage.set('Failed to load booking details.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.errorMessage.set('Invalid booking ID.');
      this.isLoading.set(false);
    }
  }

  processPayment() {
    const b = this.booking();
    if (!b) return;

    this.isProcessing.set(true);

    const request = {
      amount: b.totalAmount,
      paymentMethod: this.paymentMethod(),
      transactionId: 'TXN-' + Math.random().toString(36).substring(2, 10).toUpperCase()
    };

    this.bookingService.processPayment(b.id, request).subscribe({
      next: () => {
        this.isProcessing.set(false);
        alert('Payment successful! Your booking is confirmed.');
        this.router.navigate(['/bookings']);
      },
      error: (err) => {
        console.error(err);
        this.isProcessing.set(false);
        alert(err.error?.message || 'Payment failed. Please try again.');
      }
    });
  }
}
