import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, OnInit, OnDestroy, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { BookingService } from '../../services/booking.service';
import { BookingResponse } from '../../models/booking.model';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './payment.html'
})
export class PaymentComponent implements OnInit, OnDestroy {
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookingService = inject(BookingService);
  private toastService = inject(ToastService);

  booking = signal<BookingResponse | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  // Timer fields
  timeLeft = signal<number>(300);
  timerInterval: any;

  // Dummy payment fields
  paymentMethod = signal<string>('GPay');

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      // Find the booking in user's bookings to get the total amount
      this.bookingService.getMyBookings().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (bookings) => {
          const found = bookings.find(b => b.id === id);
          if (found) {
            let bookedAtStr = found.bookedAt;
            if (!bookedAtStr.endsWith('Z')) {
              bookedAtStr += 'Z';
            }
            const timePassed = Date.now() - new Date(bookedAtStr).getTime();
            const timeRemaining = Math.floor((300000 - timePassed) / 1000);
            
            if (found.status === 'Cancelled' || timeRemaining <= 0) {
              this.toastService.show('Your booking was cancelled due to timeout.', 'error');
              this.router.navigate(['/package', found.packageId]);
              return;
            }

            this.booking.set(found);
            this.timeLeft.set(timeRemaining);
            this.startTimer();
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

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  startTimer() {
    this.timerInterval = setInterval(() => {
      if (this.timeLeft() > 0) {
        this.timeLeft.update(v => v - 1);
      } else {
        clearInterval(this.timerInterval);
        this.toastService.show('Your booking was cancelled due to timeout.', 'error');
        this.router.navigate(['/package', this.booking()?.packageId]);
      }
    }, 1000);
  }

  formatTime(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  }

  processPayment() {
    const b = this.booking();
    if (!b) return;

    this.isProcessing.set(true);
    if (this.timerInterval) clearInterval(this.timerInterval);

    const request = {
      amount: b.totalAmount,
      paymentMethod: this.paymentMethod(),
      transactionId: 'TXN-' + Math.random().toString(36).substring(2, 10).toUpperCase()
    };

    this.bookingService.processPayment(b.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isProcessing.set(false);
        this.router.navigate(['/bookings']);
      },
      error: (err) => {
        console.error(err);
        this.isProcessing.set(false);
        this.toastService.show(err.error?.message || 'Payment failed. Please try again.', 'error');
        this.startTimer(); // resume timer on failure
      }
    });
  }
}
