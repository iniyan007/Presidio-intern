import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ManageBookingsComponent } from './manage-bookings';
import { PackageService } from '../../services/package.service';
import { BookingService } from '../../services/booking.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { ActivatedRoute } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { DomSanitizer } from '@angular/platform-browser';

describe('ManageBookingsComponent', () => {
  let component: ManageBookingsComponent;
  let fixture: ComponentFixture<ManageBookingsComponent>;

  let packageServiceSpy: any;
  let bookingServiceSpy: any;
  let userServiceSpy: any;
  let toastServiceSpy: any;
  let routeQueryParams: BehaviorSubject<any>;

  beforeEach(async () => {
    packageServiceSpy = {
      getMyPackages: vi.fn().mockReturnValue(of([])),
      getPackageById: vi.fn().mockReturnValue(of({}))
    };
    bookingServiceSpy = {
      getBookingsByPackageId: vi.fn().mockReturnValue(of([])),
      verifyDocument: vi.fn().mockReturnValue(of({})),
      verifyBooking: vi.fn().mockReturnValue(of({}))
    };
    userServiceSpy = {
      userProfile: signal(null)
    };
    toastServiceSpy = {
      show: vi.fn()
    };
    routeQueryParams = new BehaviorSubject({});

    await TestBed.configureTestingModule({
      imports: [ManageBookingsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: PackageService, useValue: packageServiceSpy },
        { provide: BookingService, useValue: bookingServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        { provide: ActivatedRoute, useValue: { queryParams: routeQueryParams.asObservable() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageBookingsComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create and handle empty packages', () => {
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(packageServiceSpy.getMyPackages).toHaveBeenCalled();
    expect(component.recentAllBookings().length).toBe(0);
  });

  it('should auto-select package if packageId query param exists', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([{ id: 'pkg-1', title: 'Pkg 1' }]));
    routeQueryParams.next({ packageId: 'pkg-1' });
    
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(component.selectedPackageId()).toBe('pkg-1');
    expect(bookingServiceSpy.getBookingsByPackageId).toHaveBeenCalledWith('pkg-1');
    expect(packageServiceSpy.getPackageById).toHaveBeenCalledWith('pkg-1');
  });

  it('should load all recent bookings when no package is selected', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([{ id: 'pkg-1', title: 'Pkg 1' }, { id: 'pkg-2', title: 'Pkg 2' }]));
    
    bookingServiceSpy.getBookingsByPackageId.mockImplementation((id: string) => {
      if (id === 'pkg-1') {
        return of([{ id: 'b1', bookedAt: '2023-01-02' }]);
      }
      return of([{ id: 'b2', bookedAt: '2023-01-01' }]);
    });

    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(component.recentAllBookings().length).toBe(2);
    expect(component.recentAllBookings()[0].id).toBe('b1'); // Newer
    expect(component.recentAllBookings()[1].id).toBe('b2'); // Older
  });

  it('should handle error in getBookingsByPackageId for all recent bookings', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([{ id: 'pkg-1', title: 'Pkg 1' }]));
    bookingServiceSpy.getBookingsByPackageId.mockReturnValue(throwError(() => new Error('error')));

    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(component.recentAllBookings().length).toBe(0);
    expect(component.isLoadingAll()).toBeFalsy();
  });

  it('should group bookings correctly when a package is selected', () => {
    packageServiceSpy.getPackageById.mockReturnValue(of({
      seasonalPricings: [{ startDate: '2023-01-01', seasonName: 'Winter' }]
    }));
    bookingServiceSpy.getBookingsByPackageId.mockReturnValue(of([
      { id: 'b1', travelDate: '2023-01-01', status: 'Pending', paymentStatus: 'Paid' },
      { id: 'b2', travelDate: '2023-01-01', status: 'Cancelled', paymentStatus: 'Unpaid' }, // Should be ignored
      { id: 'b3', travelDate: '2023-02-01', status: 'Pending', paymentStatus: 'Paid' }
    ]));
    
    fixture.detectChanges();
    component.onPackageSelect('pkg-1');
    
    const groups = component.groupedBookings();
    expect(groups.length).toBe(2);
    
    // Sorts by descending travelDate, so Feb comes first
    expect(groups[0].travelDate).toBe('2023-02-01');
    expect(groups[0].seasonName).toBeUndefined();
    expect(groups[0].bookings.length).toBe(1);

    expect(groups[1].travelDate).toBe('2023-01-01');
    expect(groups[1].seasonName).toBe('Winter');
    expect(groups[1].bookings.length).toBe(1);
    expect(groups[1].bookings[0].id).toBe('b1');
  });

  it('should handle error when loading bookings for specific package', () => {
    bookingServiceSpy.getBookingsByPackageId.mockReturnValue(throwError(() => new Error('error')));
    
    fixture.detectChanges();
    component.onPackageSelect('pkg-1');
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to load bookings.', 'error');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle document viewer and verification', () => {
    fixture.detectChanges();
    const doc = { id: 'doc-1', filePath: '/test.pdf', status: 'Pending' };
    
    component.openDocumentViewer(doc as any, 'Alice');
    expect(component.selectedDocument()).toEqual({ doc, travelerName: 'Alice' });
    
    // Reject without reason
    component.verifyDocument(false);
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Please provide a rejection reason.', 'error');
    
    // Reject with reason
    component.rejectionReason.set('Blurry');
    component.verifyDocument(false);
    expect(bookingServiceSpy.verifyDocument).toHaveBeenCalledWith('doc-1', { isVerified: false, rejectionReason: 'Blurry' });
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Document successfully rejected.', 'success');
    expect(component.selectedDocument()).toBeNull();
    
    // Accept
    component.openDocumentViewer(doc as any, 'Alice');
    component.verifyDocument(true);
    expect(bookingServiceSpy.verifyDocument).toHaveBeenCalledWith('doc-1', { isVerified: true, rejectionReason: null });
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Document successfully verified.', 'success');
  });

  it('should handle document verification error', () => {
    bookingServiceSpy.verifyDocument.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    component.openDocumentViewer({ id: 'doc-1' } as any, 'Alice');
    component.verifyDocument(true);
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to verify document.', 'error');
  });

  it('should refresh specific package bookings after verification if selected', () => {
    fixture.detectChanges();
    component.selectedPackageId.set('pkg-1');
    component.openDocumentViewer({ id: 'doc-1' } as any, 'Alice');
    
    const loadSpy = vi.spyOn(component as any, 'loadBookingsForPackage');
    component.verifyDocument(true);
    
    expect(loadSpy).toHaveBeenCalledWith('pkg-1');
  });

  it('should evaluate canConfirmBooking correctly', () => {
    fixture.detectChanges();
    
    expect(component.canConfirmBooking({ status: 'Confirmed' } as any)).toBeFalsy();
    
    const pendingBooking = {
      status: 'DocumentUnderReview',
      travelers: [
        { documents: [{ status: 'Pending' }] }
      ]
    };
    expect(component.canConfirmBooking(pendingBooking as any)).toBeFalsy();
    
    const verifiedBooking = {
      status: 'DocumentUnderReview',
      travelers: [
        { documents: [{ status: 'Verified' }, { status: 'Verified' }] },
        { documents: [] }
      ]
    };
    expect(component.canConfirmBooking(verifiedBooking as any)).toBeTruthy();
    
    const noDocsBooking = {
      status: 'DocumentUnderReview',
      travelers: []
    };
    expect(component.canConfirmBooking(noDocsBooking as any)).toBeFalsy();
  });

  it('should handle booking confirmation success and error', () => {
    fixture.detectChanges();
    
    // Success
    component.confirmBooking('b-1');
    expect(bookingServiceSpy.verifyBooking).toHaveBeenCalledWith('b-1');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Booking confirmed successfully.', 'success');
    
    // Error
    bookingServiceSpy.verifyBooking.mockReturnValue(throwError(() => new Error('error')));
    component.confirmBooking('b-1');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to confirm booking.', 'error');
  });

  it('should format document URL safely', () => {
    fixture.detectChanges();
    const sanitizer = TestBed.inject(DomSanitizer);
    vi.spyOn(sanitizer, 'bypassSecurityTrustResourceUrl');
    
    const docRel = { filePath: '/doc.pdf' } as any;
    component.getSafeDocumentUrl(docRel);
    expect(sanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalled();
  });
});
