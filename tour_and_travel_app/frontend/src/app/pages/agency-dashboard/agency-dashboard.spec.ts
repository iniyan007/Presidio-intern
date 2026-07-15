import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AgencyDashboardComponent } from './agency-dashboard';
import { PackageService } from '../../services/package.service';
import { BookingService } from '../../services/booking.service';
import { UserService } from '../../services/user.service';
import { AgencyService } from '../../services/agency.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';

describe('AgencyDashboardComponent', () => {
  let component: AgencyDashboardComponent;
  let fixture: ComponentFixture<AgencyDashboardComponent>;

  let packageServiceSpy: any;
  let bookingServiceSpy: any;
  let userServiceSpy: any;
  let packagerServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    packageServiceSpy = {
      getMyPackages: vi.fn().mockReturnValue(of([]))
    };
    bookingServiceSpy = {
      getBookingsByPackageId: vi.fn().mockReturnValue(of([]))
    };
    userServiceSpy = {
      userProfile: signal(null)
    };
    packagerServiceSpy = {
      getMyPackagerStatus: vi.fn().mockReturnValue(of({ companyName: 'Test Agency', deactivatedAt: null }))
    };

    await TestBed.configureTestingModule({
      imports: [AgencyDashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: PackageService, useValue: packageServiceSpy },
        { provide: BookingService, useValue: bookingServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: AgencyService, useValue: packagerServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AgencyDashboardComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load dashboard data when user profile is set', () => {
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();

    expect(component.packagerId()).toBe('user-1');
    expect(component.packagerName()).toBe('Test Agency'); // overwritten by getMyPackagerStatus
    expect(packagerServiceSpy.getMyPackagerStatus).toHaveBeenCalled();
    expect(packageServiceSpy.getMyPackages).toHaveBeenCalled();
  });

  it('should handle deactivation info in packager status', () => {
    packagerServiceSpy.getMyPackagerStatus.mockReturnValue(of({ deactivatedAt: '2023-01-01', reason: 'Violation' }));
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();

    expect(component.deactivationInfo()).toEqual({ deactivatedAt: '2023-01-01', reason: 'Violation' });
  });

  it('should handle 0 packages correctly', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([]));
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();

    expect(component.myPackages().length).toBe(0);
    expect(component.totalPackagesCount()).toBe(0);
    expect(component.recentBookings().length).toBe(0);
    expect(bookingServiceSpy.getBookingsByPackageId).not.toHaveBeenCalled();
  });

  it('should calculate stats correctly with packages and bookings', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([
      { id: 'pkg1', title: 'Package 1', totalReviews: 1, avgRating: 4, startingPrice: 100, primaryImageUrl: '/img1.jpg' },
      { id: 'pkg2', title: 'Package 2', totalReviews: 0, startingPrice: 200, primaryImageUrl: 'http://img2.jpg' }
    ]));
    
    bookingServiceSpy.getBookingsByPackageId.mockImplementation((id: string) => {
      if (id === 'pkg1') {
        return of([
          { id: 'b1', travelers: [{ fullName: 'Alice' }], totalAmount: 100, paymentStatus: 'Paid', status: 'Confirmed', bookedAt: '2023-01-01' },
          { id: 'b2', travelers: [{ fullName: 'Bob' }], totalAmount: 50, paymentStatus: 'Pending', status: 'DocumentUnderReview', bookedAt: '2023-01-02' }
        ]);
      }
      return of([]);
    });

    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();

    expect(component.totalPackagesCount()).toBe(2);
    expect(component.myPackages().length).toBe(2);
    expect(component.myPackages()[0].imageUrl).toContain('/img1.jpg');
    expect(component.myPackages()[1].imageUrl).toBe('http://img2.jpg');
    
    expect(component.avgRating()).toBe(4); // Only pkg1 has reviews
    
    expect(component.totalRevenue()).toBe(100); // b1 is Paid & Confirmed
    expect(component.pendingApprovals()).toBe(1); // b2 is DocumentUnderReview
    
    expect(component.recentBookings().length).toBe(2);
    expect(component.recentBookings()[0].id).toBe('b2'); // newer
  });

  it('should handle error when fetching bookings for a package', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([{ id: 'pkg1' }]));
    bookingServiceSpy.getBookingsByPackageId.mockReturnValue(throwError(() => new Error('error')));

    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();

    // even on error, it should increment completedFetches and finish setup without blocking
    expect(component.recentBookings().length).toBe(0);
  });

  it('should handle view all modal', () => {
    fixture.detectChanges();
    
    component.openViewAllModal();
    expect(component.isViewAllModalOpen()).toBeTruthy();
    
    component.closeViewAllModal();
    expect(component.isViewAllModalOpen()).toBeFalsy();
  });

  it('should navigate on booking click', () => {
    fixture.detectChanges();
    component.isViewAllModalOpen.set(true);
    
    component.onBookingClick({ packageId: 'pkg1' });
    
    expect(component.isViewAllModalOpen()).toBeFalsy();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/agency/manage-bookings'], { queryParams: { packageId: 'pkg1' } });
  });

  it('should navigate on edit package', () => {
    fixture.detectChanges();
    component.onEditPackage('pkg1');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/agency/edit-package', 'pkg1']);
  });
});
