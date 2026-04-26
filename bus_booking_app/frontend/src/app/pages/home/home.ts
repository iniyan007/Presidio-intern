import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TripService } from '../../services/trip';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  source: string = '';
  destination: string = '';
  date: string = '';
  trips: any[] = [];
  loading: boolean = false;
  sortBy: string = 'earliest';

  constructor(
    private tripService: TripService, 
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.search();
  }

  search() {
    console.log('Search triggered with:', { source: this.source, dest: this.destination, date: this.date });
    this.loading = true;
    this.tripService.searchTrips(this.source, this.destination, this.date).subscribe({
      next: (res) => {
        this.trips = res;
        this.sortTrips();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  sortTrips() {
    if (!this.trips || this.trips.length === 0) return;

    this.trips.sort((a, b) => {
      switch (this.sortBy) {
        case 'price_low':
          return a.totalPrice - b.totalPrice;
        case 'price_high':
          return b.totalPrice - a.totalPrice;
        case 'seats_high':
          return b.availableSeats - a.availableSeats;
        case 'earliest':
        default:
          return new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime();
      }
    });
  }

  bookNow(tripId: number) {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.router.navigate(['/trip', tripId]);
  }
}
