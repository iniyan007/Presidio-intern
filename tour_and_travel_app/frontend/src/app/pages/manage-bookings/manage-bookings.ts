import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, effect, inject, signal, DestroyRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PackageService } from '../../services/package.service';
import { BookingService } from '../../services/booking.service';
import { UserService } from '../../services/user.service';
import { BookingResponse, BookingTravelerResponse, TravelDocumentResponse } from '../../models/booking.model';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../environments/environment';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-manage-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-bookings.html',
  styleUrl: './manage-bookings.css'
})
export class ManageBookingsComponent {
  private destroyRef = inject(DestroyRef);
  private packageService = inject(PackageService);
  private bookingService = inject(BookingService);
  private userService = inject(UserService);
  private toastService = inject(ToastService);
  private sanitizer = inject(DomSanitizer);
  private route = inject(ActivatedRoute);

  myPackages = signal<any[]>([]);
  selectedPackageId = signal<string | null>(null);
  
  // Bookings grouped by seasonal price / travel date
  groupedBookings = signal<{ travelDate: string, seasonName?: string, bookings: BookingResponse[] }[]>([]);
  isLoading = signal<boolean>(false);

  // Recent bookings across all packages
  recentAllBookings = signal<(BookingResponse & { packageTitle?: string })[]>([]);
  isLoadingAll = signal<boolean>(false);

  // Document Viewer Modal State
  selectedDocument = signal<{ doc: TravelDocumentResponse, travelerName: string } | null>(null);
  rejectionReason = signal<string>('');

  constructor() {
    effect(() => {
      const profile = this.userService.userProfile();
      if (profile && profile.fullName) {
        this.loadPackages();
      }
    });
  }

  private loadPackages() {
    this.packageService.getMyPackages().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        const pkgs = res;
        this.myPackages.set(pkgs);
        
        // If a packageId is passed in query params, auto-select it
        this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
          const pid = params['packageId'];
          if (pid && pkgs.find((p: any) => p.id === pid)) {
            this.onPackageSelect(pid);
          } else {
            this.loadAllRecentBookings(pkgs);
          }
        });
      }
    });
  }

  private loadAllRecentBookings(packages: any[]) {
    this.isLoadingAll.set(true);
    let allBookings: any[] = [];
    let completedFetches = 0;

    if (packages.length === 0) {
      this.recentAllBookings.set([]);
      this.isLoadingAll.set(false);
      return;
    }

    packages.forEach((pkg: any) => {
      this.bookingService.getBookingsByPackageId(pkg.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (bookings: BookingResponse[]) => {
          const formattedBookings = bookings.map(b => ({ ...b, packageTitle: pkg.title }));
          allBookings.push(...formattedBookings);
          completedFetches++;
          if (completedFetches === packages.length) {
            allBookings.sort((a, b) => new Date(b.bookedAt).getTime() - new Date(a.bookedAt).getTime());
            this.recentAllBookings.set(allBookings.slice(0, 15)); // Show top 15 recent bookings
            this.isLoadingAll.set(false);
          }
        },
        error: () => {
          completedFetches++;
          if (completedFetches === packages.length) {
            allBookings.sort((a, b) => new Date(b.bookedAt).getTime() - new Date(a.bookedAt).getTime());
            this.recentAllBookings.set(allBookings.slice(0, 15));
            this.isLoadingAll.set(false);
          }
        }
      });
    });
  }

  onPackageSelect(packageId: string) {
    this.selectedPackageId.set(packageId);
    this.loadBookingsForPackage(packageId);
  }


  private loadBookingsForPackage(packageId: string) {
    this.isLoading.set(true);
    
    forkJoin({
      bookings: this.bookingService.getBookingsByPackageId(packageId),
      pkgDetails: this.packageService.getPackageById(packageId)
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        const bookings = res.bookings;
        const pkgDetails = res.pkgDetails;
        
        // Group by travel date, excluding cancelled AND unpaid bookings
        const grouped: { [key: string]: BookingResponse[] } = {};
        
        const validBookings = bookings.filter((b: any) => !(b.status === 'Cancelled' && b.paymentStatus === 'Unpaid'));

        validBookings.forEach((b: any) => {
          if (!grouped[b.travelDate]) {
            grouped[b.travelDate] = [];
          }
          grouped[b.travelDate].push(b);
        });

        const groupedArray = Object.keys(grouped).map(date => {
          // Find season name matching travelDate
          const season = pkgDetails.seasonalPricings?.find((s: any) => s.startDate === date);
          const seasonName = season ? season.seasonName : undefined;

          return {
            travelDate: date,
            seasonName: seasonName,
            bookings: grouped[date]
          };
        }).sort((a, b) => new Date(b.travelDate).getTime() - new Date(a.travelDate).getTime());

        this.groupedBookings.set(groupedArray);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.show('Failed to load bookings.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  getDocumentUrl(doc: TravelDocumentResponse): string {
    return doc.filePath.startsWith('http') ? doc.filePath : `${environment.baseUrl}${doc.filePath}`;
  }

  getSafeDocumentUrl(doc: TravelDocumentResponse): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.getDocumentUrl(doc));
  }

  openDocumentViewer(doc: TravelDocumentResponse, travelerName: string) {
    this.selectedDocument.set({ doc, travelerName });
    this.rejectionReason.set('');
  }

  closeDocumentViewer() {
    this.selectedDocument.set(null);
  }

  verifyDocument(isVerified: boolean) {
    const data = this.selectedDocument();
    if (!data) return;

    if (!isVerified && !this.rejectionReason().trim()) {
      this.toastService.show('Please provide a rejection reason.', 'error');
      return;
    }

    const request = {
      isVerified,
      rejectionReason: isVerified ? null : this.rejectionReason().trim()
    };

    this.bookingService.verifyDocument(data.doc.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show(`Document successfully ${isVerified ? 'verified' : 'rejected'}.`, 'success');
        this.closeDocumentViewer();
        if (this.selectedPackageId()) {
          this.loadBookingsForPackage(this.selectedPackageId()!); // Refresh specific package
        } else {
          this.loadAllRecentBookings(this.myPackages()); // Refresh all recent
        }
      },
      error: () => {
        this.toastService.show('Failed to verify document.', 'error');
      }
    });
  }

  canConfirmBooking(booking: BookingResponse): boolean {
    if (booking.status !== 'DocumentUnderReview' && booking.status !== 'Pending') {
      return false;
    }

    // Must have at least one traveler with documents
    let hasDocuments = false;
    for (const traveler of booking.travelers || []) {
      if (traveler.documents && traveler.documents.length > 0) {
        hasDocuments = true;
        for (const doc of traveler.documents) {
          if (doc.status !== 'Verified') {
            return false;
          }
        }
      }
    }
    
    return hasDocuments; // All documents exist and are verified
  }

  confirmBooking(bookingId: string) {
    this.bookingService.verifyBooking(bookingId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show('Booking confirmed successfully.', 'success');
        if (this.selectedPackageId()) {
          this.loadBookingsForPackage(this.selectedPackageId()!);
        } else {
          this.loadAllRecentBookings(this.myPackages());
        }
      },
      error: () => {
        this.toastService.show('Failed to confirm booking.', 'error');
      }
    });
  }
}
