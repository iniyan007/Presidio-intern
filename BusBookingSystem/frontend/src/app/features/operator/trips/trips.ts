import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-operator-trips',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './trips.html',
  styleUrl: './trips.css'
})
export class TripsComponent implements OnInit {
  api = inject(ApiService);
  fb = inject(FormBuilder);

  trips: any[] = [];
  buses: any[] = [];
  routes: any[] = [];
  showModal = false;
  
  tripForm: FormGroup = this.fb.group({
    busId: ['', Validators.required],
    routeId: ['', Validators.required],
    departureTime: ['', Validators.required],
    arrivalTime: ['', Validators.required]
  });

  ngOnInit() {
    this.fetchData();
  }

  fetchData() {
    this.api.getOperatorBuses().subscribe(res => {
      this.buses = res.filter((b: any) => b.isApproved);
      
      this.api.getAllTrips().subscribe(trips => {
        // Filter trips to only those belonging to operator's buses
        this.trips = trips.filter(t => this.buses.some(b => b.name === t.busName));
      });
    });

    this.api.getRoutes().subscribe(res => {
      this.routes = res;
    });
  }

  onSubmit() {
    if (this.tripForm.valid) {
      this.api.createTrip(this.tripForm.value).subscribe({
        next: () => {
          this.showModal = false;
          this.tripForm.reset();
          this.fetchData();
          alert('Trip created successfully');
        },
        error: (err) => {
          alert(err.error?.message || err.error || 'Failed to create trip');
        }
      });
    }
  }

  deleteTrip(id: number) {
    if(confirm('Are you sure you want to cancel this trip? Users will be refunded.')) {
      this.api.deleteTrip(id).subscribe({
        next: () => {
          this.trips = this.trips.filter(t => t.tripId !== id);
        },
        error: (err) => {
          alert(err.error?.message || err.error || 'Failed to delete trip');
        }
      });
    }
  }
}
