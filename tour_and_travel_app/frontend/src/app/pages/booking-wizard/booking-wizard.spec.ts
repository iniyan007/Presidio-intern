import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { of } from 'rxjs';

import { BookingWizardComponent } from './booking-wizard';
import { PackageService } from '../../services/package.service';
import { BookingService } from '../../services/booking.service';
import { ToastService } from '../../services/toast.service';

describe('BookingWizardComponent', () => {
  let component: BookingWizardComponent;
  let fixture: ComponentFixture<BookingWizardComponent>;
  let packageServiceMock: any;
  let bookingServiceMock: any;
  let toastServiceMock: any;

  beforeEach(async () => {
    packageServiceMock = {
      getPackageById: vi.fn().mockReturnValue(of({
        id: 'p1',
        packageType: 'Family',
        country: 'India',
        seasonalPricings: [
          { id: 's1', startDate: '2027-01-01T00:00:00Z', endDate: '2027-01-31T00:00:00Z', availableSlots: 10, basePrice: 1000 }
        ]
      }))
    };

    bookingServiceMock = {
      getPlatformConfig: vi.fn().mockReturnValue(of({ platformFeePercent: 2, gstPercent: 18 })),
      createBooking: vi.fn().mockReturnValue(of({ id: 'b1' }))
    };

    toastServiceMock = {
      show: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [BookingWizardComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: PackageService, useValue: packageServiceMock },
        { provide: BookingService, useValue: bookingServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: ActivatedRoute, useValue: { 
            snapshot: { 
              paramMap: { get: () => 'p1' },
              queryParamMap: { get: () => 's1' }
            } 
          } 
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingWizardComponent);
    component = fixture.componentInstance;
    
    // Prevent actual scroll
    window.scrollTo = vi.fn();
    
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should calculate traveler limits properly based on package type (Family)', () => {
    expect(component.maxAllowedTravelers()).toBe(10);
  });

  it('should enforce Honeymoon rules', async () => {
    // Switch to honeymoon package
    packageServiceMock.getPackageById.mockReturnValue(of({
      id: 'p1',
      packageType: 'Honeymoon',
      country: 'India',
      seasonalPricings: [
        { id: 's1', startDate: '2027-01-01T00:00:00Z', endDate: '2027-01-31T00:00:00Z', availableSlots: 10, basePrice: 1000 }
      ]
    }));
    
    component.travelers.set([
      { fullName: 'Primary', dateOfBirth: '', gender: 'Male', nationality: 'Indian', passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian', isPrimary: true, passportFile: null, aadharFile: null },
      { fullName: 'Secondary', dateOfBirth: '', gender: 'Female', nationality: 'Indian', passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian', isPrimary: false, passportFile: null, aadharFile: null }
    ]);
    
    component.ngOnInit();
    await fixture.whenStable();
    
    // Max allowed slots for honeymoon should be 2 even if season has 10
    expect(component.maxAllowedTravelers()).toBe(2);
    
    // Try to remove traveler to have 1 traveler (Honeymoon needs exactly 2)
    component.removeTraveler(1);
    expect(toastServiceMock.show).toHaveBeenCalledWith('Honeymoon packages require exactly 2 travelers.', 'error');
  });

  it('should block navigation to step 2 if age < 18 on primary traveler', () => {
    // Current date - 10 years
    const childDob = new Date();
    childDob.setFullYear(childDob.getFullYear() - 10);
    
    component.travelers.update(t => {
      const updated = [...t];
      updated[0].dateOfBirth = childDob.toISOString().split('T')[0];
      updated[0].fullName = 'Child';
      updated[0].aadharCardNumber = '123412341234';
      return updated;
    });
    
    component.setStep(2);
    
    expect(toastServiceMock.show).toHaveBeenCalledWith('Traveler 1 (Primary): Must be at least 18 years old to book.', 'error');
    expect(component.step()).toBe(1); // Blocked
  });

  it('should calculate pricing correctly', () => {
    // Component initialized with 1 primary traveler by default (adult).
    // Let's add 1 child traveler to test calculations.
    component.travelers.set([
      { fullName: 'Adult', dateOfBirth: '1990-01-01', gender: 'Male', nationality: 'Indian', passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian', isPrimary: true, passportFile: null, aadharFile: null },
      { fullName: 'Child', dateOfBirth: '2020-01-01', gender: 'Female', nationality: 'Indian', passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian', isPrimary: false, passportFile: null, aadharFile: null }
    ]);
    
    // Season basePrice = 1000. No child price is explicitly mocked, so it defaults to basePrice = 1000.
    // Total base = 1000 * 2 = 2000.
    // Platform fee = 2%, GST = 18%.
    
    const pricing = component.pricing();
    expect(pricing.baseTotal).toBe(2000);
    expect(pricing.discount).toBe(0);
    // After discount = 2000
    // Platform fee = 2000 * 0.02 = 40
    expect(pricing.platformFee).toBe(40);
    // GST = (2000 + 40) * 0.18 = 2040 * 0.18 = 367.2
    expect(pricing.gst).toBe(367.2);
    // Grand total = 2000 + 40 + 367.2 = 2407.2
    expect(pricing.grandTotal).toBe(2407.2);
  });

  it('should not allow adding more travelers than max limit', () => {
    // Max is 10 for Family, we have 1 by default, let's add up to 10
    component.travelers.set(Array(10).fill({
      fullName: 'Traveler', dateOfBirth: '1990-01-01', gender: 'Male', nationality: 'Indian', passportNumber: '', aadharCardNumber: '', mealPreference: 'Vegetarian', isPrimary: false, passportFile: null, aadharFile: null
    }));
    
    component.addTraveler(); // Trying to add 11th
    
    expect(toastServiceMock.show).toHaveBeenCalledWith('Cannot add more travelers. Only 10 slots are allowed for this package/season.', 'error');
    expect(component.travelers().length).toBe(10);
  });

  it('should save and clear draft properly', () => {
    component.travelDate.set('2027-01-15');
    component.saveDraft();
    
    const draftKey = `bookingDraft_p1`;
    const savedDraft = localStorage.getItem(draftKey);
    expect(savedDraft).toBeTruthy();
    expect(JSON.parse(savedDraft as string).travelDate).toBe('2027-01-15');
    
    component.clearDraft();
    expect(localStorage.getItem(draftKey)).toBeNull();
  });

  it('should block step 2 if required fields are missing', () => {
    // Missing Aadhar card number which is mandatory
    component.travelers.update(t => {
      const updated = [...t];
      updated[0].fullName = 'Valid Name';
      updated[0].dateOfBirth = '1990-01-01';
      updated[0].aadharCardNumber = ''; // Missing
      return updated;
    });
    
    component.setStep(2);
    
    expect(toastServiceMock.show).toHaveBeenCalledWith('Traveler 1: Please fill out all mandatory fields (Name, DOB, Aadhar).', 'error');
    expect(component.step()).toBe(1);
  });

  it('should successfully submit booking and navigate to payment', () => {
    const routerMock = TestBed.inject(Router);
    vi.spyOn(routerMock, 'navigate');
    
    component.travelDate.set('2027-01-10'); // Valid date
    component.submitBooking();
    
    expect(component.isSubmitting()).toBe(false);
    expect(bookingServiceMock.createBooking).toHaveBeenCalled();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/payment', 'b1']);
  });
});
