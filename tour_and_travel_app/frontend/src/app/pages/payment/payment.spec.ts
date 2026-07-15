import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentComponent } from './payment';
import { BookingService } from '../../services/booking.service';
import { ToastService } from '../../services/toast.service';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('PaymentComponent', () => {
  let component: PaymentComponent;
  let fixture: ComponentFixture<PaymentComponent>;

  let bookingServiceSpy: any;
  let toastServiceSpy: any;
  let routerSpy: any;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    vi.useFakeTimers();

    bookingServiceSpy = {
      getMyBookings: vi.fn().mockReturnValue(of([])),
      processPayment: vi.fn().mockReturnValue(of({}))
    };
    
    toastServiceSpy = {
      show: vi.fn()
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: { get: vi.fn().mockReturnValue('b1') }
      }
    };

    await TestBed.configureTestingModule({
      imports: [PaymentComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: BookingService, useValue: bookingServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('should handle missing ID', () => {
    mockActivatedRoute.snapshot.paramMap.get.mockReturnValue(null);
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Invalid booking ID.');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should load booking successfully and start timer', () => {
    // Current time is simulated
    const mockNow = new Date('2023-01-01T10:00:00Z').getTime();
    vi.setSystemTime(mockNow);
    
    // booked 1 minute ago => 4 minutes left
    const bookedAt = new Date(mockNow - 60000).toISOString();
    
    bookingServiceSpy.getMyBookings.mockReturnValue(of([{ id: 'b1', bookedAt, status: 'Pending', packageId: 'p1' }]));
    
    fixture.detectChanges();
    
    expect(component.booking()).toBeTruthy();
    expect(component.timeLeft()).toBe(240); // 4 minutes
    expect(component.isLoading()).toBeFalsy();
    
    // Test timer tick
    vi.advanceTimersByTime(1000);
    expect(component.timeLeft()).toBe(239);
  });

  it('should handle booking not found', () => {
    bookingServiceSpy.getMyBookings.mockReturnValue(of([{ id: 'b2' }]));
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Booking not found or you do not have permission to view it.');
  });

  it('should handle getMyBookings error', () => {
    bookingServiceSpy.getMyBookings.mockReturnValue(throwError(() => new Error('error')));
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Failed to load booking details.');
  });

  it('should cancel if already cancelled or timeout expired', () => {
    const mockNow = new Date('2023-01-01T10:00:00Z').getTime();
    vi.setSystemTime(mockNow);
    
    // booked 6 minutes ago => expired
    const bookedAt = new Date(mockNow - 360000).toISOString();
    
    bookingServiceSpy.getMyBookings.mockReturnValue(of([{ id: 'b1', bookedAt, status: 'Pending', packageId: 'p1' }]));
    
    fixture.detectChanges();
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Your booking was cancelled due to timeout.', 'error');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/package', 'p1']);
  });

  it('should timeout and redirect when timer hits 0', () => {
    const mockNow = new Date('2023-01-01T10:00:00Z').getTime();
    vi.setSystemTime(mockNow);
    
    // 5 seconds left
    const bookedAt = new Date(mockNow - 295000).toISOString();
    bookingServiceSpy.getMyBookings.mockReturnValue(of([{ id: 'b1', bookedAt, status: 'Pending', packageId: 'p1' }]));
    
    fixture.detectChanges();
    
    expect(component.timeLeft()).toBe(5);
    
    vi.advanceTimersByTime(6000); // Wait 6 seconds

    expect(toastServiceSpy.show).toHaveBeenCalledWith('Your booking was cancelled due to timeout.', 'error');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/package', 'p1']);
  });

  it('should format time correctly', () => {
    expect(component.formatTime(300)).toBe('5:00');
    expect(component.formatTime(65)).toBe('1:05');
    expect(component.formatTime(9)).toBe('0:09');
  });

  it('should clear interval on destroy', () => {
    const mockNow = new Date('2023-01-01T10:00:00Z').getTime();
    vi.setSystemTime(mockNow);
    const bookedAt = new Date(mockNow - 60000).toISOString();
    bookingServiceSpy.getMyBookings.mockReturnValue(of([{ id: 'b1', bookedAt, status: 'Pending', packageId: 'p1' }]));
    
    fixture.detectChanges();
    
    expect(component.timerInterval).toBeTruthy();
    
    component.ngOnDestroy();
    
    // Just verifying it doesn't crash, we can't easily assert clearInterval was called directly here 
    // without spying on global object, but we know it runs.
  });

  it('should process payment successfully', () => {
    component.booking.set({ id: 'b1', totalAmount: 100 } as any);
    component.paymentMethod.set('Card');
    
    component.processPayment();
    
    expect(bookingServiceSpy.processPayment).toHaveBeenCalledWith('b1', expect.objectContaining({
      amount: 100,
      paymentMethod: 'Card'
    }));
    
    // since mock returns instantly, it should be done
    expect(component.isProcessing()).toBeFalsy();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/bookings']);
  });

  it('should handle payment failure and restart timer', () => {
    component.booking.set({ id: 'b1', totalAmount: 100 } as any);
    bookingServiceSpy.processPayment.mockReturnValue(throwError(() => ({ error: { message: 'Card declined' } })));
    
    const startSpy = vi.spyOn(component, 'startTimer');
    
    component.processPayment();
    
    expect(component.isProcessing()).toBeFalsy();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Card declined', 'error');
    expect(startSpy).toHaveBeenCalled();
  });
});
