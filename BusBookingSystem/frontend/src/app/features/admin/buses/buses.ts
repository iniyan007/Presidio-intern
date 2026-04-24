import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-buses',
  imports: [CommonModule],
  templateUrl: './buses.html',
  styleUrl: './buses.css'
})
export class BusesComponent implements OnInit {
  api = inject(ApiService);
  buses: any[] = [];
  loading = true;

  ngOnInit() {
    this.fetchBuses();
  }

  fetchBuses() {
    this.loading = true;
    this.api.getAllBuses().subscribe(res => {
      this.buses = res;
      this.loading = false;
    });
  }

  approveBus(id: number) {
    this.api.approveBus(id).subscribe(() => {
      this.fetchBuses();
    });
  }

  rejectBus(id: number) {
    if(confirm('Are you sure you want to reject this bus?')) {
      this.api.rejectBus(id).subscribe(() => {
        this.fetchBuses();
      });
    }
  }
}
