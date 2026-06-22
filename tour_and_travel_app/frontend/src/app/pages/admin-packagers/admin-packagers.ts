import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-admin-packagers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-packagers.html',
})
export class AdminPackagersComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);

  activeTab = signal<'active' | 'deactivated'>('active');
  packagers = signal<any[]>([]);
  isLoading = signal<boolean>(true);

  searchTerm = signal<string>('');
  sortOrder = signal<string>('newest');
  private searchSubject = new Subject<string>();

  activeDeactivateRowId = signal<string | null>(null);
  deactivateReason = signal<string>('');
  isDeactivating = signal<boolean>(false);
  
  activeActivateRowId = signal<string | null>(null);
  isActivating = signal<boolean>(false);

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.loadPackagers();
    });

    this.loadPackagers();
  }

  setTab(tab: 'active' | 'deactivated') {
    this.activeTab.set(tab);
    this.searchTerm.set('');
    this.sortOrder.set('newest');
    this.loadPackagers();
  }

  loadPackagers() {
    this.isLoading.set(true);
    if (this.activeTab() === 'active') {
      this.adminService.getApprovedPackagers(this.searchTerm(), this.sortOrder()).subscribe({
        next: (res: any) => {
          this.packagers.set(Array.isArray(res) ? res : (res.data || []));
          this.isLoading.set(false);
        },
        error: () => {
          this.toastService.show('Failed to load active packagers', 'error');
          this.isLoading.set(false);
        }
      });
    } else {
      this.adminService.getDeactivatedPackagers(this.searchTerm(), this.sortOrder()).subscribe({
        next: (res: any) => {
          this.packagers.set(Array.isArray(res) ? res : (res.data || []));
          this.isLoading.set(false);
        },
        error: () => {
          this.toastService.show('Failed to load deactivated packagers', 'error');
          this.isLoading.set(false);
        }
      });
    }
  }

  onSearchChange(term: string) {
    this.searchTerm.set(term);
    this.searchSubject.next(term);
  }

  toggleSort() {
    this.sortOrder.set(this.sortOrder() === 'newest' ? 'oldest' : 'newest');
    this.loadPackagers();
  }

  startDeactivation(id: string) {
    this.activeDeactivateRowId.set(id);
    this.deactivateReason.set('');
  }

  cancelDeactivation() {
    this.activeDeactivateRowId.set(null);
    this.deactivateReason.set('');
  }

  confirmDeactivation(id: string) {
    const reason = this.deactivateReason().trim();
    if (!reason) {
      this.toastService.show('Please provide a reason for deactivation.', 'error');
      return;
    }

    this.isDeactivating.set(true);
    this.adminService.deactivatePackager(id, reason).subscribe({
      next: () => {
        this.toastService.show('Packager deactivated successfully', 'success');
        this.isDeactivating.set(false);
        this.cancelDeactivation();
        this.loadPackagers(); // Refresh the list
      },
      error: (err) => {
        console.error(err);
        this.toastService.show('Failed to deactivate packager', 'error');
        this.isDeactivating.set(false);
      }
    });
  }

  startActivation(id: string) {
    this.activeActivateRowId.set(id);
  }

  cancelActivation() {
    this.activeActivateRowId.set(null);
  }

  confirmActivation(id: string) {
    this.isActivating.set(true);
    this.adminService.activatePackager(id).subscribe({
      next: () => {
        this.toastService.show('Packager activated successfully', 'success');
        this.isActivating.set(false);
        this.cancelActivation();
        this.loadPackagers(); // Refresh the list
      },
      error: (err) => {
        console.error(err);
        this.toastService.show('Failed to activate packager', 'error');
        this.isActivating.set(false);
      }
    });
  }
}
