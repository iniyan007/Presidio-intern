import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { WishlistService } from '../../services/wishlist.service';
import { TravelPackageDetails, PackageMedia, PackageReview, PackageSeasonalPricing } from '../../models/package.model';
import { AuthService } from '../../services/auth.service';
import { BookingService } from '../../services/booking.service';
import { ReviewModalComponent } from '../../components/review-modal/review-modal';

@Component({
  selector: 'app-package-details',
  standalone: true,
  imports: [CommonModule, RouterModule, ReviewModalComponent],
  templateUrl: './package-details.html',
  styleUrl: './package-details.css'
})
export class PackageDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private packageService = inject(PackageService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private bookingService = inject(BookingService);
  wishlistService = inject(WishlistService);

  pkg = signal<TravelPackageDetails | null>(null);
  reviews = signal<PackageReview[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string>('');
  isLoggedIn = signal<boolean>(false);
  showReviewModal = signal<boolean>(false);
  eligibleBookingId = signal<string | null>(null);
  selectedSeason = signal<PackageSeasonalPricing | null>(null);

  ngOnInit() {
    this.isLoggedIn.set(!!this.authService.getToken());
    if (!this.isLoggedIn()) {
      this.router.navigate(['/auth']);
      return;
    } else {
      this.wishlistService.loadWishlists();
    }

    const packageId = this.route.snapshot.paramMap.get('id');
    if (packageId) {
      this.loadPackage(packageId);
    } else {
      this.errorMessage.set('Invalid package ID.');
      this.isLoading.set(false);
    }
  }

  loadPackage(id: string) {
    this.isLoading.set(true);
    this.packageService.getPackageById(id).subscribe({
      next: (data) => {
        this.pkg.set(data);
        this.loadReviews(id);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Failed to load package details.');
        this.isLoading.set(false);
      }
    });
  }

  loadReviews(packageId: string) {
    this.packageService.getPackageReviews(packageId).subscribe({
      next: (res) => {
        this.reviews.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load reviews', err);
        // Do not block UI if reviews fail
        this.isLoading.set(false);
      }
    });
  }

  getPrimaryImage(): string {
    const p = this.pkg();
    if (!p || !p.media || p.media.length === 0) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE';
    }
    const primary = p.media.find(m => m.isPrimary) || p.media[0];
    if (!primary || !primary.filePath) return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE';
    return primary.filePath.startsWith('http') ? primary.filePath : `http://localhost:5082${primary.filePath}`;
  }

  getGalleryImages(): string[] {
    const p = this.pkg();
    if (!p || !p.media) return [];
    // exclude primary if possible, or just return top 3
    const primary = p.media.find(m => m.isPrimary) || p.media[0];
    return p.media.filter(m => m.id !== primary?.id && m.filePath)
                  .slice(0, 3)
                  .map(m => m.filePath.startsWith('http') ? m.filePath : `http://localhost:5082${m.filePath}`);
  }

  getStartingPrice(): number {
    const p = this.pkg();
    if (!p || !p.seasonalPricings || p.seasonalPricings.length === 0) return 0;
    return Math.min(...p.seasonalPricings.map(sp => sp.basePrice));
  }

  checkReviewEligibilityAndOpen() {
    const packageId = this.pkg()?.id;
    if (!packageId) return;

    this.bookingService.getMyBookings().subscribe({
      next: (bookings) => {
        // Find a booking for this package that is Confirmed or Completed
        const eligibleBooking = bookings.find(b => 
          b.packageId === packageId && 
          (b.status === 'Confirmed' || b.status === 'Completed')
        );

        if (eligibleBooking) {
          this.eligibleBookingId.set(eligibleBooking.id);
          this.showReviewModal.set(true);
        } else {
          this.toastService.show('You must have a Confirmed or Completed booking for this package before writing a review.', 'error');
        }
      },
      error: () => {
        this.toastService.show('Please log in to write a review.', 'error');
      }
    });
  }

  onReviewSubmitted() {
    this.showReviewModal.set(false);
    this.toastService.show('Review submitted successfully! Thank you for your feedback.', 'success');
    if (this.pkg()?.id) {
      this.loadReviews(this.pkg()!.id);
      this.loadPackage(this.pkg()!.id); // Reload package to update averages
    }
  }
}
