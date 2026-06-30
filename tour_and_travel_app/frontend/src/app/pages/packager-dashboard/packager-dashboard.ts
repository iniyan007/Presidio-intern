import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, effect, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PackageService } from '../../services/package.service';
import { BookingService } from '../../services/booking.service';
import { UserService } from '../../services/user.service';
import { PackagerService } from '../../services/packager.service';
import { environment } from '../../../environments/environment';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-packager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './packager-dashboard.html',
  styleUrl: './packager-dashboard.css'
})
export class PackagerDashboardComponent {
  private destroyRef = inject(DestroyRef);
  totalRevenue = signal<number>(0);
  revenueGrowth = signal<number>(0);
  pendingApprovals = signal<number>(0);
  avgRating = signal<number>(0);

  myPackages = signal<any[]>([]);
  totalPackagesCount = signal<number>(0);
  recentBookings = signal<any[]>([]);
  actionableBookings = signal<any[]>([]);
  isViewAllModalOpen = signal(false);
  packagerId = signal<string | null>(null);
  packagerName = signal<string | null>(null);
  deactivationInfo = signal<{ deactivatedAt: string, reason: string } | null>(null);

  private packageService = inject(PackageService);
  private bookingService = inject(BookingService);
  private userService = inject(UserService);
  private packagerService = inject(PackagerService);
  private router = inject(Router);

  constructor() {
    effect(() => {
      const profile = this.userService.userProfile();
      if (profile && profile.fullName) {
        this.packagerId.set(profile.id || null);
        this.packagerName.set(profile.fullName);
        this.loadDashboardData(profile.fullName);
      }
    });
  }

  private loadDashboardData(packagerName: string) {
    this.packagerService.getMyPackagerStatus().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (status) => {
        if (status.deactivatedAt) {
          this.deactivationInfo.set({ deactivatedAt: status.deactivatedAt, reason: status.reason || 'No specific reason provided.' });
        }
      }
    });
    this.packageService.getMyPackages().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        const packages = res; // getMyPackages returns an array directly
        
        const packageList = packages.map((pkg: any) => {
          let imgUrl = 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE'; // default
          if (pkg.primaryImageUrl) {
            imgUrl = pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `${environment.baseUrl}${pkg.primaryImageUrl}`;
          }

          return {
            id: pkg.id,
            title: pkg.title,
            durationDays: pkg.durationDays,
            status: pkg.status || 'Active', // Use actual status, fallback to Active
            slotsLeft: pkg.pendingSeats || 0,
            price: pkg.startingPrice,
            imageUrl: imgUrl
          };
        });

        this.totalPackagesCount.set(packageList.length);
        this.myPackages.set(packageList.slice(0, 4));

        // Calculate average rating
        const totalRating = packages.reduce((sum: number, pkg: any) => sum + (pkg.avgRating || 0), 0);
        this.avgRating.set(packages.length ? totalRating / packages.length : 0);

        // Fetch bookings for each package
        let allBookings: any[] = [];
        let completedFetches = 0;
        let revenue = 0;
        let pending = 0;

        if (packages.length === 0) {
          this.recentBookings.set([]);
          return;
        }

        packages.forEach((pkg: any) => {
          this.bookingService.getBookingsByPackageId(pkg.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
            next: (bookings: any[]) => {
              bookings.forEach(b => {
                allBookings.push({
                  id: b.id,
                  customerName: b.travelers?.[0]?.fullName || 'Unknown Customer',
                  initials: (b.travelers?.[0]?.fullName || 'U').substring(0, 2).toUpperCase(),
                  packageId: pkg.id,
                  packageTitle: pkg.title,
                  amount: b.totalAmount,
                  status: b.paymentStatus, // e.g. 'Paid', 'Pending'
                  bookingStatus: b.status, // e.g. 'DocumentUnderReview'
                  bgColor: b.paymentStatus === 'Paid' ? 'bg-secondary-fixed' : 'bg-primary-fixed',
                  textColor: b.paymentStatus === 'Paid' ? 'text-secondary' : 'text-primary',
                  createdAt: b.bookedAt || b.createdAt,
                  cancellationReason: b.cancellationReason
                });

                if (b.paymentStatus === 'Paid') revenue += b.totalAmount;
                if (b.status === 'DocumentUnderReview') pending++;
              });

              completedFetches++;
              if (completedFetches === packages.length) {
                this.totalRevenue.set(revenue);
                this.pendingApprovals.set(pending);
                
                // Sort bookings by created date descending and take top 5 for recent
                allBookings.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                this.recentBookings.set(allBookings.slice(0, 5));
                
                // Set actionable bookings (Paid & DocumentUnderReview)
                this.actionableBookings.set(allBookings.filter(b => b.status === 'Paid' && b.bookingStatus === 'DocumentUnderReview'));
              }
            },
            error: () => {
              completedFetches++;
              if (completedFetches === packages.length) {
                this.totalRevenue.set(revenue);
                this.pendingApprovals.set(pending);
                allBookings.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                this.recentBookings.set(allBookings.slice(0, 5));
                this.actionableBookings.set(allBookings.filter(b => b.status === 'Paid' && b.bookingStatus === 'DocumentUnderReview'));
              }
            }
          });
        });
      }
    });
  }

  onBookingClick(booking: any) {
    this.isViewAllModalOpen.set(false);
    this.router.navigate(['/packager/manage-bookings'], { queryParams: { packageId: booking.packageId } });
  }

  openViewAllModal() {
    this.isViewAllModalOpen.set(true);
  }

  closeViewAllModal() {
    this.isViewAllModalOpen.set(false);
  }

  onEditPackage(packageId: string) {
    this.router.navigate(['/packager/edit-package', packageId]);
  }
}
