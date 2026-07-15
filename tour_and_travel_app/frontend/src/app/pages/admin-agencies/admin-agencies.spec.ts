import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminAgenciesComponent } from './admin-agencies';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { ChatService } from '../../services/chat.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { provideRouter } from '@angular/router';

describe('AdminAgenciesComponent', () => {
  let component: AdminAgenciesComponent;
  let fixture: ComponentFixture<AdminAgenciesComponent>;

  let adminServiceSpy: any;
  let toastServiceSpy: any;
  let chatServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    adminServiceSpy = {
      getApprovedPackagers: vi.fn().mockReturnValue(of({ data: [] })),
      getDeactivatedPackagers: vi.fn().mockReturnValue(of({ data: [] })),
      deactivatePackager: vi.fn(),
      activatePackager: vi.fn(),
      getPackagerDocuments: vi.fn()
    };
    toastServiceSpy = { show: vi.fn() };
    chatServiceSpy = { getOrInitializeThread: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [AdminAgenciesComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AdminService, useValue: adminServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        { provide: ChatService, useValue: chatServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminAgenciesComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
    
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('should create and load active packagers on init', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(adminServiceSpy.getApprovedPackagers).toHaveBeenCalled();
  });

  it('should handle search input and debounce', () => {
    fixture.detectChanges();
    adminServiceSpy.getApprovedPackagers.mockClear();
    
    component.onSearchChange('test');
    
    expect(adminServiceSpy.getApprovedPackagers).not.toHaveBeenCalled();
    
    vi.advanceTimersByTime(300);
    
    expect(adminServiceSpy.getApprovedPackagers).toHaveBeenCalledWith('test', 'newest');
  });

  it('should switch tabs and reload packagers', () => {
    fixture.detectChanges();
    
    component.setTab('deactivated');
    
    expect(component.activeTab()).toBe('deactivated');
    expect(component.searchTerm()).toBe('');
    expect(adminServiceSpy.getDeactivatedPackagers).toHaveBeenCalled();
  });

  it('should handle toggle sort', () => {
    fixture.detectChanges();
    
    expect(component.sortOrder()).toBe('newest');
    component.toggleSort();
    expect(component.sortOrder()).toBe('oldest');
    expect(adminServiceSpy.getApprovedPackagers).toHaveBeenCalledWith('', 'oldest');
  });

  it('should handle error when loading packagers', () => {
    adminServiceSpy.getApprovedPackagers.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to load active agencies', 'error');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should manage deactivation state', () => {
    component.startDeactivation('123');
    expect(component.activeDeactivateRowId()).toBe('123');
    expect(component.deactivateReason()).toBe('');

    component.cancelDeactivation();
    expect(component.activeDeactivateRowId()).toBeNull();
  });

  it('should confirm deactivation successfully', () => {
    adminServiceSpy.deactivatePackager.mockReturnValue(of({}));
    component.deactivateReason.set('Violation');
    
    component.confirmDeactivation('123');
    
    expect(adminServiceSpy.deactivatePackager).toHaveBeenCalledWith('123', 'Violation');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Packager deactivated successfully', 'success');
    expect(component.activeDeactivateRowId()).toBeNull();
  });

  it('should handle empty deactivation reason', () => {
    component.deactivateReason.set('   ');
    component.confirmDeactivation('123');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Please provide a reason for deactivation.', 'error');
    expect(adminServiceSpy.deactivatePackager).not.toHaveBeenCalled();
  });

  it('should handle deactivation error', () => {
    adminServiceSpy.deactivatePackager.mockReturnValue(throwError(() => new Error('error')));
    component.deactivateReason.set('Violation');
    
    component.confirmDeactivation('123');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to deactivate packager', 'error');
    expect(component.isDeactivating()).toBeFalsy();
  });

  it('should manage activation state', () => {
    component.startActivation('123');
    expect(component.activeActivateRowId()).toBe('123');

    component.cancelActivation();
    expect(component.activeActivateRowId()).toBeNull();
  });

  it('should confirm activation successfully', () => {
    adminServiceSpy.activatePackager.mockReturnValue(of({}));
    
    component.confirmActivation('123');
    
    expect(adminServiceSpy.activatePackager).toHaveBeenCalledWith('123');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Packager activated successfully', 'success');
    expect(component.activeActivateRowId()).toBeNull();
  });

  it('should handle activation error', () => {
    adminServiceSpy.activatePackager.mockReturnValue(throwError(() => new Error('error')));
    
    component.confirmActivation('123');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to activate packager', 'error');
    expect(component.isActivating()).toBeFalsy();
  });

  it('should start chat and navigate', () => {
    chatServiceSpy.getOrInitializeThread.mockReturnValue(of({ id: 'thread-123' }));
    
    component.startChat('packager-1');
    
    expect(chatServiceSpy.getOrInitializeThread).toHaveBeenCalledWith({ packagerId: 'packager-1' });
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/chat'], { queryParams: { threadId: 'thread-123' } });
  });

  it('should handle error when starting chat', () => {
    chatServiceSpy.getOrInitializeThread.mockReturnValue(throwError(() => new Error('error')));
    
    component.startChat('packager-1');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to start chat with packager', 'error');
  });

  it('should open docs modal and load docs', () => {
    adminServiceSpy.getPackagerDocuments.mockReturnValue(of([{ id: 1 }]));
    
    component.openDocsModal({ id: 'pkg-1' });
    
    expect(component.isDocsModalOpen()).toBeTruthy();
    expect(component.selectedPackagerForDocs()).toEqual({ id: 'pkg-1' });
    expect(adminServiceSpy.getPackagerDocuments).toHaveBeenCalledWith('pkg-1');
    expect(component.packagerDocuments()).toEqual([{ id: 1 }]);
    expect(component.isDocumentsLoading()).toBeFalsy();
  });

  it('should handle error when loading docs', () => {
    adminServiceSpy.getPackagerDocuments.mockReturnValue(throwError(() => new Error('error')));
    
    component.openDocsModal({ id: 'pkg-1' });
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to fetch documents', 'error');
    expect(component.isDocumentsLoading()).toBeFalsy();
  });

  it('should close docs modal', () => {
    component.isDocsModalOpen.set(true);
    component.selectedPackagerForDocs.set({});
    
    component.closeDocsModal();
    
    expect(component.isDocsModalOpen()).toBeFalsy();
    expect(component.selectedPackagerForDocs()).toBeNull();
  });

  it('should view document in new tab', () => {
    const openSpy = vi.spyOn(window, 'open').mockImplementation(() => null);
    
    component.viewDocument('http://example.com/doc.pdf');
    expect(openSpy).toHaveBeenCalledWith('http://example.com/doc.pdf', '_blank');
    
    component.viewDocument('/api/doc.pdf');
    expect(openSpy).toHaveBeenCalledWith(expect.stringContaining('/api/doc.pdf'), '_blank');
  });
});
