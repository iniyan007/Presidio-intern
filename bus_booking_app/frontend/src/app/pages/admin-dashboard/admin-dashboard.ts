import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  activeTab: string = 'operators'; // 'operators', 'buses', or 'routes'
  stats: any = { totalPlatformRevenue: 0, pendingOperators: 0, pendingBuses: 0, totalBookings: 0 };
  operators: any[] = [];
  buses: any[] = [];
  routes: any[] = [];
  
  routeForm: FormGroup;
  message: string = '';

  constructor(
    private fb: FormBuilder, 
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) {
    this.routeForm = this.fb.group({
      source: ['', Validators.required],
      destination: ['', Validators.required],
      distance: [0, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    this.loadAll();
  }

  loadAll() {
    this.loadStats();
    this.loadOperators();
    this.loadBuses();
    this.loadRoutes();
  }

  loadStats() {
    this.adminService.getStats().subscribe(res => {
      this.stats = res;
      this.cdr.detectChanges();
    });
  }

  loadOperators() {
    this.adminService.getAllOperators().subscribe({
      next: (res) => {
        this.operators = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error fetching operators:', err)
    });
  }

  loadBuses() {
    this.adminService.getAllBuses().subscribe(res => {
      this.buses = res;
      this.cdr.detectChanges();
    });
  }

  loadRoutes() {
    this.adminService.getRoutes().subscribe(res => {
      this.routes = res;
      this.cdr.detectChanges();
    });
  }

  approveOperator(id: number) {
    this.adminService.approveOperator(id).subscribe({
      next: () => {
        this.message = 'Operator approved!';
        this.loadAll();
        this.cdr.detectChanges();
        setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
      },
      error: (err) => console.error(err)
    });
  }

  rejectOperator(id: number) {
    if (confirm('Are you sure you want to reject/remove this operator?')) {
      this.adminService.rejectOperator(id).subscribe({
        next: () => {
          this.message = 'Operator removed.';
          this.loadAll();
          this.cdr.detectChanges();
          setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
        },
        error: (err) => console.error(err)
      });
    }
  }

  approveBus(id: number) {
    this.adminService.approveBus(id).subscribe({
      next: () => {
        this.message = 'Bus approved!';
        this.loadAll();
        this.cdr.detectChanges();
        setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
      },
      error: (err) => console.error(err)
    });
  }

  rejectBus(id: number) {
    if (confirm('Are you sure you want to reject/remove this bus?')) {
      this.adminService.rejectBus(id).subscribe({
        next: () => {
          this.message = 'Bus removed.';
          this.loadAll();
          this.cdr.detectChanges();
          setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
        },
        error: (err) => console.error(err)
      });
    }
  }

  onSubmitRoute() {
    if (this.routeForm.invalid) return;
    this.adminService.createRoute(this.routeForm.value).subscribe({
      next: () => {
        this.message = 'Route created successfully!';
        this.routeForm.reset();
        this.loadRoutes();
        this.loadStats();
        this.cdr.detectChanges();
        setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
      },
      error: (err) => {
        this.message = err.error?.message || 'Error creating route';
        this.cdr.detectChanges();
        setTimeout(() => { this.message = ''; this.cdr.detectChanges(); }, 3000);
      }
    });
  }

  deleteRoute(id: number) {
    if (confirm('Delete this route?')) {
      this.adminService.deleteRoute(id).subscribe({
        next: () => {
          this.loadRoutes();
        },
        error: (err) => console.error(err)
      });
    }
  }
}
