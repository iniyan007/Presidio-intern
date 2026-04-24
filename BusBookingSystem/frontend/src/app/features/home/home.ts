import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api';

@Component({
  selector: 'app-home',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent implements OnInit {
  fb = inject(FormBuilder);
  router = inject(Router);
  api = inject(ApiService);

  searchForm!: FormGroup;
  routes: any[] = [];
  sources: string[] = [];
  destinations: string[] = [];

  ngOnInit() {
    this.searchForm = this.fb.group({
      source: ['', Validators.required],
      destination: ['', Validators.required],
      date: ['', Validators.required]
    });

    this.loadRoutes();
  }

  loadRoutes() {
    this.api.getRoutes().subscribe(res => {
      this.routes = res;
      // Extract unique sources and destinations
      this.sources = [...new Set(res.map(r => r.source))].sort() as string[];
      this.destinations = [...new Set(res.map(r => r.destination))].sort() as string[];
    });
  }

  onSearch() {
    if (this.searchForm.valid) {
      const { source, destination, date } = this.searchForm.value;
      this.router.navigate(['/search'], { queryParams: { source, destination, date } });
    } else {
      this.searchForm.markAllAsTouched();
    }
  }
}
