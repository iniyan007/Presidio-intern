import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-seat-selection',
  imports: [CommonModule],
  templateUrl: './seat-selection.html',
  styleUrl: './seat-selection.css'
})
export class SeatSelectionComponent implements OnInit {
  route = inject(ActivatedRoute);
  router = inject(Router);
  api = inject(ApiService);

  tripId!: number;
  seats: any[] = [];
  selectedSeats: number[] = [];
  loading = true;

  locking = false;
  booking = false;
  step = 1; // 1: Select Seat, 2: Payment

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.tripId = +params['tripId'];
      this.fetchSeats();
    });
  }

  fetchSeats() {
    this.api.getSeats(this.tripId).subscribe(res => {
      this.seats = res;
      this.loading = false;
    });
  }

  toggleSeat(seatId: number, status: string) {
    if (status !== 'AVAILABLE') return;
    
    const index = this.selectedSeats.indexOf(seatId);
    if (index === -1) {
      if (this.selectedSeats.length >= 4) {
        alert('You can select maximum 4 seats at a time.');
        return;
      }
      this.selectedSeats.push(seatId);
    } else {
      this.selectedSeats.splice(index, 1);
    }
  }

  isSelected(seatId: number) {
    return this.selectedSeats.includes(seatId);
  }

  proceedToPayment() {
    if (this.selectedSeats.length === 0) return;
    this.locking = true;
    this.api.lockSeat(this.tripId, this.selectedSeats).subscribe({
      next: () => {
        this.locking = false;
        this.step = 2; // Move to payment dummy screen
      },
      error: (err) => {
        this.locking = false;
        alert(err.error?.message || 'Error locking seats. They might have been taken.');
        this.fetchSeats(); // Refresh seats
        this.selectedSeats = [];
      }
    });
  }

  confirmBooking() {
    this.booking = true;
    this.api.createBooking(this.tripId, this.selectedSeats).subscribe({
      next: () => {
        this.booking = false;
        alert('Booking successful!');
        this.router.navigate(['/bookings']);
      },
      error: (err) => {
        this.booking = false;
        alert(err.error?.message || 'Error during booking.');
        this.step = 1;
        this.fetchSeats();
        this.selectedSeats = [];
      }
    });
  }

  cancelPayment() {
    this.step = 1;
    this.fetchSeats();
    this.selectedSeats = [];
  }
}
