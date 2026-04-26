import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TripService } from '../../services/trip';
import { BookingService } from '../../services/booking';

@Component({
  selector: 'app-trip-details',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './trip-details.html',
  styleUrl: './trip-details.css'
})
export class TripDetailsComponent implements OnInit {
  tripId: number = 0;
  trip: any = null;
  selectedSeats: string[] = [];
  seats: any[][] = [];
  step: number = 1; // 1: Seats, 2: Passengers, 3: Payment, 4: Success
  bookingForm: FormGroup;
  bookingId: number = 0;
  timeLeft: number = 420; // 7 minutes
  timerInterval: any;
  paymentTimeLeft: number = 60; // 1 minute
  paymentTimerInterval: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private tripService: TripService,
    private bookingService: BookingService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.tripId = Number(this.route.snapshot.paramMap.get('id'));
    this.bookingForm = this.fb.group({
      passengers: this.fb.array([])
    });
  }

  get passengers() {
    return this.bookingForm.get('passengers') as FormArray;
  }

  ngOnInit() {
    this.loadTrip();
  }

  loadTrip() {
    this.tripService.getTripDetails(this.tripId).subscribe({
      next: (res) => {
        console.log('Trip details loaded:', res);
        this.trip = res;
        if (!this.trip.bookedSeats) this.trip.bookedSeats = [];
        this.generateSeatLayout();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading trip:', err);
        this.cdr.detectChanges();
      }
    });
  }

  generateSeatLayout() {
    const totalSeats = this.trip.bus.totalSeats;
    const rows = Math.ceil(totalSeats / 4);
    this.seats = [];
    
    for (let i = 0; i < rows; i++) {
      const row = [];
      for (let j = 0; j < 4; j++) {
        const seatNum = String.fromCharCode(65 + i) + (j + 1);
        row.push({
          id: seatNum,
          isBooked: this.trip.bookedSeats.includes(seatNum),
          isSelected: false
        });
      }
      this.seats.push(row);
    }
  }

  toggleSeat(seat: any) {
    if (seat.isBooked) return;
    
    if (seat.isSelected) {
      seat.isSelected = false;
      this.selectedSeats = this.selectedSeats.filter(s => s !== seat.id);
    } else {
      seat.isSelected = true;
      this.selectedSeats.push(seat.id);
    }
  }

  proceedToDetails() {
    if (this.selectedSeats.length === 0) return;

    this.bookingService.lockSeats(this.tripId, this.selectedSeats).subscribe({
      next: (res) => {
        this.bookingId = res.bookingId;
        this.step = 2;
        this.initPassengerForm();
        this.startTimer();
        this.cdr.detectChanges();
      },
      error: (err) => alert(err.error?.message || 'Error locking seats')
    });
  }

  initPassengerForm() {
    this.passengers.clear();
    this.selectedSeats.forEach(seat => {
      this.passengers.push(this.fb.group({
        seatNumber: [seat],
        name: ['', Validators.required],
        age: ['', [Validators.required, Validators.min(1)]],
        gender: ['', Validators.required]
      }));
    });
  }

  startTimer() {
    this.timeLeft = 420;
    this.timerInterval = setInterval(() => {
      this.timeLeft--;
      this.cdr.detectChanges();
      if (this.timeLeft <= 0) {
        this.clearAllTimers();
        alert('Time expired! Your seats have been released.');
        this.router.navigate(['/']);
      }
    }, 1000);
  }

  get formattedTime() {
    const mins = Math.floor(this.timeLeft / 60);
    const secs = this.timeLeft % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  startPaymentTimer() {
    this.paymentTimeLeft = 60;
    this.paymentTimerInterval = setInterval(() => {
      this.paymentTimeLeft--;
      this.cdr.detectChanges();
      if (this.paymentTimeLeft <= 0) {
        this.clearAllTimers();
        alert('Payment timeout! Please try booking again.');
        this.router.navigate(['/']);
      }
    }, 1000);
  }

  get formattedPaymentTime() {
    const mins = Math.floor(this.paymentTimeLeft / 60);
    const secs = this.paymentTimeLeft % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  onSubmit() {
    if (this.bookingForm.invalid) return;
    
    // Move to payment step
    this.step = 3;
    this.startPaymentTimer();
    this.cdr.detectChanges();
  }

  processPayment() {
    this.bookingService.confirmBooking(this.bookingId, this.bookingForm.value.passengers).subscribe({
      next: (res) => {
        this.clearAllTimers();
        this.step = 4;
        this.cdr.detectChanges();

        // Auto-redirect to dashboard after 5 seconds
        setTimeout(() => {
          this.router.navigate(['/user/dashboard']);
        }, 5000);
      },
      error: (err) => alert(err.error?.message || 'Error confirming booking')
    });
  }

  clearAllTimers() {
    if (this.timerInterval) clearInterval(this.timerInterval);
    if (this.paymentTimerInterval) clearInterval(this.paymentTimerInterval);
  }

  ngOnDestroy() {
    this.clearAllTimers();
  }
}
