import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { OperatorService } from '../../services/operator';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './operator-dashboard.html',
  styleUrl: './operator-dashboard.css'
})
export class OperatorDashboard implements OnInit {
  activeTab: string = 'trips'; // 'trips' or 'buses'
  stats: any = { totalRevenue: 0, totalExpenses: 0, netProfit: 0 };
  myBuses: any[] = [];
  myTrips: any[] = [];
  
  busForm: FormGroup;
  tripForm: FormGroup;
  isAddingBus: boolean = false;
  isAddingTrip: boolean = false;
  message: string = '';
  availableRoutes: any[] = [];

  constructor(
    private fb: FormBuilder, 
    private operatorService: OperatorService,
    private cdr: ChangeDetectorRef
  ) {
    this.busForm = this.fb.group({
      name: ['', Validators.required],
      busNumber: ['', Validators.required],
      totalSeats: [40, [Validators.required, Validators.min(10)]]
    });

    this.tripForm = this.fb.group({
      busId: ['', Validators.required],
      routeId: ['', Validators.required],
      departureTime: ['', Validators.required],
      arrivalTime: ['', Validators.required],
      ticketPrice: [0, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    this.loadStats();
    this.loadBuses();
    this.loadTrips();
    this.loadRoutes();
  }

  loadStats() {
    this.operatorService.getRevenueStats().subscribe({
      next: (res) => {
        this.stats = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadRoutes() {
    this.operatorService.getRoutes().subscribe({
      next: (res) => {
        this.availableRoutes = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadBuses() {
    this.operatorService.getMyBuses().subscribe({
      next: (res) => {
        console.log('Operator buses received:', res);
        this.myBuses = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadTrips() {
    this.operatorService.getMyTrips().subscribe({
      next: (res) => {
        console.log('Loaded Trips:', res);
        
        const now = new Date().getTime();
        this.myTrips = res.sort((a: any, b: any) => {
          const diffA = Math.abs(new Date(a.departureTime).getTime() - now);
          const diffB = Math.abs(new Date(b.departureTime).getTime() - now);
          return diffA - diffB;
        });
        
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error loading trips:', err)
    });
  }

  onSubmitBus() {
    if (this.busForm.invalid) return;
    this.operatorService.addBus(this.busForm.value).subscribe({
      next: (res) => {
        this.message = 'Bus added! Waiting for admin approval.';
        this.isAddingBus = false;
        this.busForm.reset({ totalSeats: 40 });
        this.loadBuses();
        setTimeout(() => this.message = '', 3000);
      },
      error: (err) => {
        this.message = err.error?.message || 'Error adding bus';
        setTimeout(() => this.message = '', 3000);
      }
    });
  }

  get approvedBuses() {
    return this.myBuses.filter(b => b.isApproved);
  }

  onSubmitTrip() {
    if (this.tripForm.invalid) return;
    
    const departureTime = new Date(this.tripForm.value.departureTime).getTime();
    const arrivalTime = new Date(this.tripForm.value.arrivalTime).getTime();
    
    if (departureTime >= arrivalTime) {
      this.message = 'Departure time must be earlier than arrival time.';
      setTimeout(() => this.message = '', 3000);
      return;
    }

    this.operatorService.createTrip(this.tripForm.value).subscribe({
      next: (res) => {
        this.message = 'Trip scheduled successfully!';
        this.isAddingTrip = false;
        this.tripForm.reset();
        this.loadTrips();
        setTimeout(() => this.message = '', 3000);
      },
      error: (err) => {
        this.message = err.error?.message || 'Error scheduling trip';
        setTimeout(() => this.message = '', 3000);
      }
    });
  }

  onDeleteTrip(id: number) {
    if (confirm('Are you sure you want to cancel this trip? All booked passengers will be notified and refunded.')) {
      this.operatorService.deleteTrip(id).subscribe({
        next: (res: any) => {
          this.message = res.message || 'Trip cancelled successfully.';
          this.loadTrips();
          this.loadStats(); // Update revenue immediately
          setTimeout(() => this.message = '', 3000);
        },
        error: (err) => {
          this.message = err.error?.message || 'Error cancelling trip';
          setTimeout(() => this.message = '', 3000);
        }
      });
    }
  }

  selectedTripBookings: any = null;
  selectedTripDetails: any = null;

  viewBookings(trip: any) {
    this.selectedTripDetails = trip;
    this.operatorService.getTripBookings(trip.id).subscribe({
      next: (res) => {
        this.selectedTripBookings = res;
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert('Error fetching bookings');
        console.error(err);
      }
    });
  }

  closeBookings() {
    this.selectedTripBookings = null;
    this.selectedTripDetails = null;
    this.cdr.detectChanges();
  }
}
