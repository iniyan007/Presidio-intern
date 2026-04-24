import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  api = inject(ApiService);
  
  totalRevenue: number = 0;
  pendingOperators: any[] = [];
  pendingBuses: any[] = [];
  loading = true;

  ngOnInit() {
    this.fetchData();
  }

  fetchData() {
    this.api.getTotalRevenue().subscribe(res => {
      this.totalRevenue = res.revenue;
    });

    this.api.getPendingOperators().subscribe(res => {
      this.pendingOperators = res;
    });

    this.api.getPendingBuses().subscribe(res => {
      this.pendingBuses = res;
      this.loading = false;
    });
  }

  approveOperator(id: number) {
    this.api.approveOperator(id).subscribe(() => {
      this.pendingOperators = this.pendingOperators.filter(o => o.id !== id);
    });
  }

  rejectOperator(id: number) {
    this.api.rejectOperator(id).subscribe(() => {
      this.pendingOperators = this.pendingOperators.filter(o => o.id !== id);
    });
  }

  approveBus(id: number) {
    this.api.approveBus(id).subscribe(() => {
      this.pendingBuses = this.pendingBuses.filter(b => b.id !== id);
    });
  }

  rejectBus(id: number) {
    this.api.rejectBus(id).subscribe(() => {
      this.pendingBuses = this.pendingBuses.filter(b => b.id !== id);
    });
  }
}
