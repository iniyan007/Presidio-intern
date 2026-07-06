import { Component, inject, OnInit, OnDestroy, signal, DestroyRef, ChangeDetectorRef, computed } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { WishlistService } from '../../services/wishlist.service';
import { TravelPackage } from '../../models/package.model';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { environment } from '../../../environments/environment';
import { LocationService, LocationSuggestion } from '../../services/location.service';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Subject } from 'rxjs';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroyRef = inject(DestroyRef);
  private packageService = inject(PackageService);
  private authService = inject(AuthService);
  toastService = inject(ToastService);
  private router = inject(Router);
  userService = inject(UserService);
  wishlistService = inject(WishlistService);
  private locationService = inject(LocationService);
  private cdr = inject(ChangeDetectorRef);

  packages = signal<TravelPackage[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string>('');

  searchDestination = signal<string>('');
  searchDate = signal<string>('');
  searchPackager = signal<string>('');
  selectedPackageType = signal<string>('');
  selectedSortBy = signal<string>('');
  
  locationSuggestions = signal<LocationSuggestion[]>([]);
  showLocationDropdown = signal<boolean>(false);
  
  private locationSearchSubject = new Subject<string>();

  isPolicyModalOpen = signal<boolean>(false);
  isCareersModalOpen = signal<boolean>(false);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  pageSize = signal<number>(9);

  visiblePages = computed(() => {
    const current = this.currentPage();
    const total = this.totalPages();
    const pages: number[] = [];
    
    if (total <= 5) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      if (current <= 3) {
        pages.push(1, 2, 3, 4, -1, total);
      } else if (current >= total - 2) {
        pages.push(1, -1, total - 3, total - 2, total - 1, total);
      } else {
        pages.push(1, -1, current - 1, current, current + 1, -1, total);
      }
    }
    return pages;
  });

  openPolicyModal(event: Event) {
    event.preventDefault();
    this.isPolicyModalOpen.set(true);
  }

  openCareersModal(event: Event) {
    event.preventDefault();
    this.isCareersModalOpen.set(true);
  }

  applyForPackager() {
    this.isCareersModalOpen.set(false);
    if (this.authService.isAuthenticated() && this.authService.getUserRole() === 'Packager') {
      this.toastService.show('You are already a registered Agency!', 'info');
    } else {
      this.router.navigate(['/apply-agency']);
    }
  }

  goToAgency(event: Event, packageId: string) {
    event.stopPropagation();
    this.router.navigate(['/agency', packageId]);
  }
  
  minDate = new Date().toISOString().split('T')[0];
  
  private pollingInterval: any;

  ngOnInit() {
    this.loadPackages();
    
    // Poll every 10 seconds to update live seat availability
    this.pollingInterval = setInterval(() => {
      this.loadPackages(true); // silent load
    }, 10000);

    if (this.authService.isAuthenticated()) {
      this.wishlistService.loadWishlists();
    }

    this.locationSearchSubject.pipe(
      takeUntilDestroyed(this.destroyRef),
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(query => {
        if (!query || query.length < 2) {
          this.locationSuggestions.set([]);
          this.showLocationDropdown.set(false);
          return [];
        }
        return this.locationService.searchLocation(query);
      })
    ).subscribe(suggestions => {
      this.locationSuggestions.set(suggestions);
      this.showLocationDropdown.set(suggestions.length > 0);
      this.cdr.detectChanges();
    });
  }

  onSearchDestinationInput(value: string) {
    this.searchDestination.set(value);
    this.locationSearchSubject.next(value);
  }

  selectLocation(suggestion: LocationSuggestion) {
    const locationName = suggestion.address.city || suggestion.address.town || suggestion.address.village || suggestion.name;
    this.searchDestination.set(locationName);
    this.showLocationDropdown.set(false);
  }

  closeLocationDropdown() {
    setTimeout(() => {
      this.showLocationDropdown.set(false);
    }, 200);
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
    if (this.searchPackager().trim()) filters.PackagerName = this.searchPackager().trim();
    if (this.selectedPackageType()) filters.PackageType = this.selectedPackageType();
    if (this.searchDate().trim()) filters.TravelStartDate = this.searchDate().trim();
    if (this.selectedSortBy()) filters.SortBy = this.selectedSortBy();
    filters.PageNumber = this.currentPage();
    filters.PageSize = this.pageSize();

    this.packageService.getPackages(filters).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        const pkgs = res.items ? res.items : (res.data ? res.data : res);
        this.packages.set(pkgs || []);
        if (res.pageNumber) this.currentPage.set(res.pageNumber);
        if (res.totalPages !== undefined) this.totalPages.set(res.totalPages);
        else if (res.totalCount !== undefined) this.totalPages.set(Math.ceil(res.totalCount / this.pageSize()) || 1);
        this.isLoading.set(false);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage.set('Failed to load packages');
        this.isLoading.set(false);
        this.cdr.detectChanges();
      }
    });
  }
  
  selectPackageType(type: string) {
    this.selectedPackageType.set(type);
    this.currentPage.set(1);
    this.loadPackages();
  }

  exploreGroupTours() {
    this.selectPackageType('Group');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onSearch() {
    this.currentPage.set(1);
    this.loadPackages();
  }

  clearFilters() {
    this.searchDestination.set('');
    this.searchDate.set('');
    this.searchPackager.set('');
    this.selectedPackageType.set('');
    this.selectedSortBy.set('');
    this.currentPage.set(1);
    this.loadPackages();
  }

  onSortChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedSortBy.set(value);
    this.currentPage.set(1);
    this.loadPackages();
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages() && page !== this.currentPage()) {
      this.currentPage.set(page);
      this.loadPackages();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  nextPage() {
    if (this.currentPage() < this.totalPages()) {
      this.goToPage(this.currentPage() + 1);
    }
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.goToPage(this.currentPage() - 1);
    }
  }

  viewDetails(packageId: string) {
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('Please log in to view package details and continue booking.', 'info');
      this.router.navigate(['/auth']);
    } else {
      this.router.navigate(['/package', packageId]);
    }
  }

  getPrimaryImage(pkg: any): string {
    if (!pkg.primaryImageUrl) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE'; // default fallback
    }
    return pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `${environment.baseUrl}${pkg.primaryImageUrl}`;
  }

  getStartingPrice(pkg: any): number {
    return pkg.startingPrice || 0;
  }
}
