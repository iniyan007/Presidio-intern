import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-operator-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  api = inject(ApiService);
  
  revenue: number = 0;
  bookings: any[] = [];
  loading = true;

  ngOnInit() {
    this.api.getOperatorRevenue().subscribe(res => {
      this.revenue = res.revenue;
    });

    this.api.getOperatorBookings().subscribe(res => {
      this.bookings = res.items || res; // Assuming paginated or list
      this.loading = false;
    });
  }
}
