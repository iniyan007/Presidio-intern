import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { ChatService } from '../../services/chat.service';
import { environment } from '../../../environments/environment';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-admin-agencies',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-agencies.html',
})
export class AdminAgenciesComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);
  private chatService = inject(ChatService);
  private router = inject(Router);

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

  // Documents Modal State
  isDocsModalOpen = signal<boolean>(false);
  selectedPackagerForDocs = signal<any | null>(null);
  packagerDocuments = signal<any[]>([]);
  isDocumentsLoading = signal<boolean>(false);

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
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
      this.adminService.getApprovedPackagers(this.searchTerm(), this.sortOrder()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (res: any) => {
          this.packagers.set(Array.isArray(res) ? res : (res.data || []));
          this.isLoading.set(false);
        },
        error: () => {
          this.toastService.show('Failed to load active agencies', 'error');
          this.isLoading.set(false);
        }
      });
    } else {
      this.adminService.getDeactivatedPackagers(this.searchTerm(), this.sortOrder()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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
    this.adminService.deactivatePackager(id, reason).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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
    this.adminService.activatePackager(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  startChat(packagerId: string) {
    this.chatService.getOrInitializeThread({ packagerId }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (thread) => {
        this.router.navigate(['/chat'], { queryParams: { threadId: thread.id } });
      },
      error: (err) => {
        console.error(err);
        this.toastService.show('Failed to start chat with packager', 'error');
      }
    });
  }

  openDocsModal(packager: any) {
    this.selectedPackagerForDocs.set(packager);
    this.isDocsModalOpen.set(true);
    this.packagerDocuments.set([]);
    this.isDocumentsLoading.set(true);

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

  closeDocsModal() {
    this.isDocsModalOpen.set(false);
    this.selectedPackagerForDocs.set(null);
  }

  viewDocument(fileUrl: string) {
    const fullUrl = fileUrl.startsWith('http') ? fileUrl : `${environment.baseUrl}${fileUrl}`;
    window.open(fullUrl, '_blank');
  }
}
