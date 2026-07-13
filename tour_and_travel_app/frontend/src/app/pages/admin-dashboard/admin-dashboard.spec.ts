import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminDashboardComponent } from './admin-dashboard';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('AdminDashboardComponent', () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;

  let adminServiceSpy: any;
  let toastServiceSpy: any;

  beforeEach(async () => {
    adminServiceSpy = {
      getAnalytics: vi.fn().mockReturnValue(of({ totalRevenue: 100, totalGst: 10, totalBookings: 5, activePackagers: 2 })),
      getPlatformConfig: vi.fn().mockReturnValue(of({ platformFeePercent: 10, gstPercent: 18 })),
      getPendingPackagers: vi.fn().mockReturnValue(of({ data: [{ id: 'pkg1' }] })),
      approvePackager: vi.fn(),
      rejectPackager: vi.fn(),
      updatePlatformConfig: vi.fn(),
      getPackagerDocuments: vi.fn()
    };
    toastServiceSpy = { show: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AdminService, useValue: adminServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    }).compileComponents();

    vi.useFakeTimers();
    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('should create and load initial data on init', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(adminServiceSpy.getAnalytics).toHaveBeenCalled();
    expect(adminServiceSpy.getPlatformConfig).toHaveBeenCalled();
    expect(adminServiceSpy.getPendingPackagers).toHaveBeenCalled();
    
    expect(component.revenue()).toBe(100);
    expect(component.gstCollected()).toBe(10);
    expect(component.bookings()).toBe(5);
    expect(component.activePackagersCount()).toBe(2);
    expect(component.platformFee()).toBe(10);
    expect(component.gstPercent()).toBe(18);
    expect(component.pendingPackagers().length).toBe(1);
  });

  it('should clear interval on destroy', () => {
    fixture.detectChanges();
    const clearIntervalSpy = vi.spyOn(globalThis, 'clearInterval');
    component.ngOnDestroy();
    expect(clearIntervalSpy).toHaveBeenCalled();
  });

  it('should handle analytics load error gracefully', () => {
    adminServiceSpy.getAnalytics.mockReturnValue(throwError(() => new Error('error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    expect(consoleSpy).toHaveBeenCalledWith('Failed to load analytics');
  });

  it('should handle config load error gracefully', () => {
    adminServiceSpy.getPlatformConfig.mockReturnValue(throwError(() => new Error('error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    expect(consoleSpy).toHaveBeenCalledWith('Failed to load config');
  });

  it('should handle pending packagers load error', () => {
    adminServiceSpy.getPendingPackagers.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to load pending packagers', 'error');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should refresh data silently on interval', () => {
    fixture.detectChanges();
    adminServiceSpy.getAnalytics.mockClear();
    adminServiceSpy.getPendingPackagers.mockClear();

    // Mock new packagers to trigger toast
    adminServiceSpy.getPendingPackagers.mockReturnValue(of({ data: [{ id: 'pkg1' }, { id: 'pkg2' }] }));
    
    vi.advanceTimersByTime(30000);
    
    expect(adminServiceSpy.getAnalytics).toHaveBeenCalled();
    expect(adminServiceSpy.getPendingPackagers).toHaveBeenCalled();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('1 new packager request(s) just arrived!', 'info');
    expect(component.pendingPackagers().length).toBe(2);
  });

  it('should not update packagers array silently if review modal is open', () => {
    fixture.detectChanges();
    component.isReviewModalOpen.set(true);
    
    adminServiceSpy.getPendingPackagers.mockReturnValue(of({ data: [{ id: 'pkg1' }, { id: 'pkg2' }] }));
    vi.advanceTimersByTime(30000);
    
    expect(component.pendingPackagers().length).toBe(1); // Unchanged
  });

  it('should handle search input and debounce', () => {
    fixture.detectChanges();
    adminServiceSpy.getPendingPackagers.mockClear();
    
    component.onSearchChange('search term');
    expect(adminServiceSpy.getPendingPackagers).not.toHaveBeenCalled();
    
    vi.advanceTimersByTime(300);
    expect(adminServiceSpy.getPendingPackagers).toHaveBeenCalledWith('search term', 'newest');
  });

  it('should handle sort toggle', () => {
    fixture.detectChanges();
    adminServiceSpy.getPendingPackagers.mockClear();
    
    component.toggleSort();
    expect(component.sortOrder()).toBe('oldest');
    expect(adminServiceSpy.getPendingPackagers).toHaveBeenCalledWith('', 'oldest');
  });

  it('should update global fee successfully', () => {
    adminServiceSpy.updatePlatformConfig.mockReturnValue(of({}));
    fixture.detectChanges();
    
    component.platformFee.set(12);
    component.gstPercent.set(15);
    component.updateGlobalFee();
    
    expect(adminServiceSpy.updatePlatformConfig).toHaveBeenCalledWith(12, 15);
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Platform settings updated successfully!', 'success');
  });

  it('should handle update global fee error', () => {
    adminServiceSpy.updatePlatformConfig.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    component.updateGlobalFee();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to update settings', 'error');
  });

  it('should approve packager', () => {
    adminServiceSpy.approvePackager.mockReturnValue(of({}));
    fixture.detectChanges();
    
    component.approvePackager('pkg1');
    expect(adminServiceSpy.approvePackager).toHaveBeenCalledWith('pkg1');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Packager approved successfully!', 'success');
    expect(component.pendingPackagers().length).toBe(0);
  });

  it('should handle approve packager error', () => {
    adminServiceSpy.approvePackager.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    component.approvePackager('pkg1');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Approval failed', 'error');
  });

  it('should toggle reject row', () => {
    fixture.detectChanges();
    
    component.toggleRejectRow('pkg1');
    expect(component.activeRejectRowId()).toBe('pkg1');
    
    component.toggleRejectRow('pkg1');
    expect(component.activeRejectRowId()).toBeNull();
  });

  it('should confirm rejection successfully', () => {
    adminServiceSpy.rejectPackager.mockReturnValue(of({}));
    fixture.detectChanges();
    
    component.activeRejectRowId.set('pkg1');
    component.rejectReason.set('Invalid details');
    component.confirmRejection('pkg1');
    
    expect(adminServiceSpy.rejectPackager).toHaveBeenCalledWith('pkg1', 'Invalid details');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Packager application rejected.', 'success');
    expect(component.pendingPackagers().length).toBe(0);
    expect(component.activeRejectRowId()).toBeNull();
  });

  it('should prevent rejection with empty reason', () => {
    fixture.detectChanges();
    component.rejectReason.set('   ');
    component.confirmRejection('pkg1');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Please provide a reason for rejection', 'error');
    expect(adminServiceSpy.rejectPackager).not.toHaveBeenCalled();
  });

  it('should handle reject packager error', () => {
    adminServiceSpy.rejectPackager.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    component.rejectReason.set('Invalid');
    component.confirmRejection('pkg1');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Rejection failed', 'error');
  });

  describe('Review Modal', () => {
    it('should open review modal and load documents', () => {
      adminServiceSpy.getPackagerDocuments.mockReturnValue(of([{ id: 'doc1', fileUrl: 'url1' }]));
      fixture.detectChanges();
      
      component.openReviewModal({ id: 'pkg1' });
      expect(component.isReviewModalOpen()).toBeTruthy();
      expect(component.selectedPackagerForReview()).toEqual({ id: 'pkg1' });
      expect(adminServiceSpy.getPackagerDocuments).toHaveBeenCalledWith('pkg1');
      expect(component.packagerDocuments().length).toBe(1);
    });

    it('should handle document load error in modal', () => {
      adminServiceSpy.getPackagerDocuments.mockReturnValue(throwError(() => new Error('error')));
      fixture.detectChanges();
      
      component.openReviewModal({ id: 'pkg1' });
      expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to fetch documents', 'error');
    });

    it('should close review modal', () => {
      fixture.detectChanges();
      component.isReviewModalOpen.set(true);
      component.closeReviewModal();
      expect(component.isReviewModalOpen()).toBeFalsy();
      expect(component.selectedPackagerForReview()).toBeNull();
    });

    it('should mark document viewed and open it', () => {
      const windowOpenSpy = vi.spyOn(window, 'open').mockImplementation(() => null);
      fixture.detectChanges();
      
      component.markDocumentViewed('doc1', 'http://example.com/doc.pdf');
      expect(component.viewedDocumentIds().has('doc1')).toBeTruthy();
      expect(windowOpenSpy).toHaveBeenCalledWith('http://example.com/doc.pdf', '_blank');
    });

    it('should calculate canApprove based on viewed documents', () => {
      fixture.detectChanges();
      
      component.packagerDocuments.set([]);
      expect(component.canApprove()).toBeFalsy();
      
      component.packagerDocuments.set([{ id: 'doc1' }, { id: 'doc2' }]);
      component.viewedDocumentIds.set(new Set(['doc1']));
      expect(component.canApprove()).toBeFalsy();
      
      component.viewedDocumentIds.set(new Set(['doc1', 'doc2']));
      expect(component.canApprove()).toBeTruthy();
    });

    it('should approve from modal if allowed', () => {
      adminServiceSpy.approvePackager.mockReturnValue(of({}));
      fixture.detectChanges();
      
      component.selectedPackagerForReview.set({ id: 'pkg1' });
      component.packagerDocuments.set([{ id: 'doc1' }]);
      component.viewedDocumentIds.set(new Set(['doc1']));
      
      component.approveFromModal();
      expect(adminServiceSpy.approvePackager).toHaveBeenCalledWith('pkg1');
      expect(component.isReviewModalOpen()).toBeFalsy();
    });

    it('should trigger reject from modal', () => {
      fixture.detectChanges();
      component.selectedPackagerForReview.set({ id: 'pkg1' });
      
      component.triggerRejectFromModal();
      expect(component.activeRejectRowId()).toBe('pkg1');
    });

    it('should confirm rejection from modal', () => {
      adminServiceSpy.rejectPackager.mockReturnValue(of({}));
      fixture.detectChanges();
      
      component.selectedPackagerForReview.set({ id: 'pkg1' });
      component.rejectReason.set('bad');
      
      component.confirmRejectionFromModal();
      expect(adminServiceSpy.rejectPackager).toHaveBeenCalledWith('pkg1', 'bad');
    });
  });
});
