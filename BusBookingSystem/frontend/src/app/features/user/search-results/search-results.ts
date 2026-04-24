import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search-results',
  imports: [CommonModule, RouterModule],
  templateUrl: './search-results.html',
  styleUrl: './search-results.css'
})
export class SearchResultsComponent implements OnInit {
  route = inject(ActivatedRoute);
  router = inject(Router);
  api = inject(ApiService);

  source: string = '';
  destination: string = '';
  date: string = '';
  
  results: any[] = [];
  loading = true;

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.source = params['source'] || '';
      this.destination = params['destination'] || '';
      this.date = params['date'] || '';

      if (this.source && this.destination) {
        this.fetchTrips();
      } else {
        this.loading = false;
      }
    });
  }

  fetchTrips() {
    this.loading = true;
    this.api.searchTrips(this.source, this.destination, this.date).subscribe({
      next: (data) => {
        this.results = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  bookTrip(tripId: number) {
    this.router.navigate(['/book', tripId]);
  }
}
