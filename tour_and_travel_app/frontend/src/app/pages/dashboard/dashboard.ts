import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PackageService, TravelPackage } from '../../services/package.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private packageService = inject(PackageService);
  private authService = inject(AuthService);
  private router = inject(Router);
  userService = inject(UserService);

  packages = signal<TravelPackage[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string>('');
  
  // Search & Filter State
  searchDestination = signal<string>('');
  searchDate = signal<string>('');
  selectedPackageType = signal<string>('');
  selectedSortBy = signal<string>('');
  
  private pollingInterval: any;

  ngOnInit() {
    this.loadPackages();
    
    // Poll every 10 seconds to update live seat availability
    this.pollingInterval = setInterval(() => {
      this.loadPackages(true); // silent load
    }, 10000);
  }

  ngOnDestroy() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  loadPackages(silent: boolean = false) {
    if (!silent) {
      this.isLoading.set(true);
    }
    
    const filters: any = {};
    if (this.searchDestination().trim()) filters.SearchTerm = this.searchDestination().trim();
    if (this.selectedPackageType()) filters.PackageType = this.selectedPackageType();
    // Optional: map searchDate if backend supports travel date filtering
    if (this.searchDate().trim()) filters.TravelStartDate = this.searchDate().trim();
    if (this.selectedSortBy()) filters.SortBy = this.selectedSortBy();

    this.packageService.getPackages(filters).subscribe({
      next: (res) => {
        // Handle pagination response or direct array
        const pkgs = res.items ? res.items : (res.data ? res.data : res);
        this.packages.set(pkgs || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Failed to load packages');
        this.isLoading.set(false);
      }
    });
  }
  
  selectPackageType(type: string) {
    this.selectedPackageType.set(type);
    this.loadPackages();
  }

  onSearch() {
    this.loadPackages();
  }

  onSortChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedSortBy.set(value);
    this.loadPackages();
  }

  viewDetails(packageId: string) {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to view package details and continue booking.');
      this.router.navigate(['/auth']);
    } else {
      this.router.navigate(['/package', packageId]);
    }
  }

  getPrimaryImage(pkg: any): string {
    if (!pkg.primaryImageUrl) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE'; // default fallback
    }
    return pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `http://localhost:5082${pkg.primaryImageUrl}`;
  }

  getStartingPrice(pkg: any): number {
    return pkg.startingPrice || 0;
  }
}
