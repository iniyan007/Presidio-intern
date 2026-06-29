import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, OnInit, OnDestroy, signal, DestroyRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../environments/environment';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './admin-dashboard.html',
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private destroyRef = inject(DestroyRef);
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);
  private pollingInterval: any;

  pendingPackagers = signal<any[]>([]);
  isLoading = signal<boolean>(true);
  activeRejectRowId = signal<string | null>(null);
  rejectReason = signal<string>('');
  isRejecting = signal<boolean>(false);
  isApproving = signal<boolean>(false);
  isReviewModalOpen = signal<boolean>(false);
  selectedPackagerForReview = signal<any | null>(null);
  packagerDocuments = signal<any[]>([]);
  viewedDocumentIds = signal<Set<string>>(new Set());
  isDocumentsLoading = signal<boolean>(false);
  revenue = signal<number>(0);
  bookings = signal<number>(0);
  activePackagersCount = signal<number>(0);
  
  platformFee = signal<number>(8.5);
  gstPercent = signal<number>(18.0);
  isUpdatingFee = signal<boolean>(false);

  searchTerm = signal<string>('');
  sortOrder = signal<string>('newest');
  private searchSubject = new Subject<string>();

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadPendingPackagers();
    });

    this.loadPendingPackagers();
    this.loadAnalytics();
    this.loadConfig();

    this.pollingInterval = setInterval(() => {
      this.refreshDataSilently();
    }, 30000);
  }

  ngOnDestroy() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  refreshDataSilently() {
    this.adminService.getAnalytics().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        this.revenue.set(res.totalRevenue || 0);
        this.bookings.set(res.totalBookings || 0);
        this.activePackagersCount.set(res.activePackagers || 0);
      }
    });

    this.adminService.getPendingPackagers(this.searchTerm(), this.sortOrder()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        const newPackagers = Array.isArray(res) ? res : (res.data || []);
        const currentCount = this.pendingPackagers().length;
        
        if (newPackagers.length > currentCount) {
          const newArrivals = newPackagers.length - currentCount;
          this.toastService.show(`${newArrivals} new packager request(s) just arrived!`, 'info');
        }
        
        // Update data silently only if we aren't actively reviewing someone to avoid weird UI shifts
        if (!this.isReviewModalOpen() && !this.activeRejectRowId()) {
            this.pendingPackagers.set(newPackagers);
        }
      }
    });
  }

  loadAnalytics() {
    this.adminService.getAnalytics().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        this.revenue.set(res.totalRevenue || 0);
        this.bookings.set(res.totalBookings || 0);
        this.activePackagersCount.set(res.activePackagers || 0);
      },
      error: () => console.error('Failed to load analytics')
    });
  }

  loadConfig() {
    this.adminService.getPlatformConfig().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        if (res) {
          this.platformFee.set(res.platformFeePercent || 0);
          this.gstPercent.set(res.gstPercent || 0);
        }
      },
      error: () => console.error('Failed to load config')
    });
  }

  onSearchChange(term: string) {
    this.searchTerm.set(term);
    this.searchSubject.next(term);
  }

  toggleSort() {
    this.sortOrder.set(this.sortOrder() === 'newest' ? 'oldest' : 'newest');
    this.loadPendingPackagers();
  }

  loadPendingPackagers() {
    this.isLoading.set(true);
    this.adminService.getPendingPackagers(this.searchTerm(), this.sortOrder()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        this.pendingPackagers.set(Array.isArray(res) ? res : (res.data || []));
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.show('Failed to load pending packagers', 'error');
        this.isLoading.set(false);
      }
    });
  }

  approvePackager(id: string) {
    this.isApproving.set(true);
    this.adminService.approvePackager(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show('Packager approved successfully!', 'success');
        this.pendingPackagers.update(list => list.filter(p => p.id !== id));
        this.isApproving.set(false);
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Approval failed', 'error');
        this.isApproving.set(false);
      }
    });
  }

  toggleRejectRow(id: string) {
    if (this.activeRejectRowId() === id) {
      this.activeRejectRowId.set(null);
      this.rejectReason.set('');
    } else {
      this.activeRejectRowId.set(id);
      this.rejectReason.set('');
    }
  }

  confirmRejection(id: string) {
    if (!this.rejectReason().trim()) {
      this.toastService.show('Please provide a reason for rejection', 'error');
      return;
    }

    this.isRejecting.set(true);
    this.adminService.rejectPackager(id, this.rejectReason()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.show('Packager application rejected.', 'success');
        this.pendingPackagers.update(list => list.filter(p => p.id !== id));
        this.activeRejectRowId.set(null);
        this.rejectReason.set('');
        this.isRejecting.set(false);
        
        // Close modal if open
        if (this.isReviewModalOpen()) {
          this.closeReviewModal();
        }
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Rejection failed', 'error');
        this.isRejecting.set(false);
      }
    });
  }

  updateGlobalFee() {
    this.isUpdatingFee.set(true);
    this.adminService.updatePlatformConfig(this.platformFee(), this.gstPercent()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isUpdatingFee.set(false);
        this.toastService.show(`Platform settings updated successfully!`, 'success');
      },
      error: (err) => {
        this.isUpdatingFee.set(false);
        this.toastService.show(err.error?.message || 'Failed to update settings', 'error');
      }
    });
  }
  openReviewModal(packager: any) {
    this.selectedPackagerForReview.set(packager);
    this.isReviewModalOpen.set(true);
    this.packagerDocuments.set([]);
    this.viewedDocumentIds.set(new Set());
    this.isDocumentsLoading.set(true);
    this.rejectReason.set('');
    this.activeRejectRowId.set(null);

    this.adminService.getPackagerDocuments(packager.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (docs) => {
        this.packagerDocuments.set(docs || []);
        this.isDocumentsLoading.set(false);
      },
      error: () => {
        this.toastService.show('Failed to fetch documents', 'error');
        this.isDocumentsLoading.set(false);
      }
    });
  }

  closeReviewModal() {
    this.isReviewModalOpen.set(false);
    this.selectedPackagerForReview.set(null);
    this.packagerDocuments.set([]);
    this.viewedDocumentIds.set(new Set());
    this.activeRejectRowId.set(null);
    this.rejectReason.set('');
  }

  markDocumentViewed(docId: string, fileUrl: string) {
    const updatedSet = new Set(this.viewedDocumentIds());
    updatedSet.add(docId);
    this.viewedDocumentIds.set(updatedSet);
    const fullUrl = fileUrl.startsWith('http') ? fileUrl : `${environment.baseUrl}${fileUrl}`;
    window.open(fullUrl, '_blank');
  }

  canApprove(): boolean {
    const docs = this.packagerDocuments();
    if (docs.length === 0) return false; // If no docs, shouldn't approve. Or maybe allow? The user said "view the document then only approve"
    return this.viewedDocumentIds().size === docs.length;
  }

  approveFromModal() {
    const packager = this.selectedPackagerForReview();
    if (packager && this.canApprove()) {
      this.approvePackager(packager.id);
      this.closeReviewModal();
    }
  }

  triggerRejectFromModal() {
    const packager = this.selectedPackagerForReview();
    if (packager) {
      this.activeRejectRowId.set(packager.id);
    }
  }

  confirmRejectionFromModal() {
    const packager = this.selectedPackagerForReview();
    if (packager) {
      this.confirmRejection(packager.id);
    }
  }
}
