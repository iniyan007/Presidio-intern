import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { of } from 'rxjs';

import { CreatePackageComponent } from './create-package';
import { PackageService } from '../../services/package.service';
import { ToastService } from '../../services/toast.service';
import { MetadataService } from '../../services/metadata.service';
import { LocationService } from '../../services/location.service';

describe('CreatePackageComponent', () => {
  let component: CreatePackageComponent;
  let fixture: ComponentFixture<CreatePackageComponent>;
  let packageServiceMock: any;
  let toastServiceMock: any;
  let metadataServiceMock: any;
  let locationServiceMock: any;
  let routerMock: any;

  beforeEach(async () => {
    packageServiceMock = {
      getMyPackageById: vi.fn().mockReturnValue(of({})),
      createPackage: vi.fn().mockReturnValue(of({ success: true })),
      updateFullPackage: vi.fn().mockReturnValue(of({ success: true }))
    };

    toastServiceMock = {
      show: vi.fn()
    };

    metadataServiceMock = {
      getCountries: vi.fn().mockReturnValue(of(['India', 'USA'])),
      getEnums: vi.fn().mockReturnValue(of({}))
    };

    locationServiceMock = {
      searchLocation: vi.fn().mockReturnValue(of([]))
    };
    
    routerMock = {
      navigate: vi.fn(),
      url: '/agency/create-package',
      events: of({})
    };

    await TestBed.configureTestingModule({
      imports: [CreatePackageComponent],
      providers: [
        provideRouter([]), 
        provideHttpClient(), 
        provideHttpClientTesting(),
        { provide: PackageService, useValue: packageServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: MetadataService, useValue: metadataServiceMock },
        { provide: LocationService, useValue: locationServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { paramMap: of({ get: () => null }) } }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreatePackageComponent);
    component = fixture.componentInstance;
    
    // Prevent actual scroll in test environment
    window.scrollTo = vi.fn();
    
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should validate start date and end date', () => {
    const spGroup = component.seasonalPricing.at(0);
    spGroup.patchValue({
      startDate: '2027-01-10',
      endDate: '2027-01-05' // Invalid, end before start
    });
    
    expect(spGroup.hasError('dateRange')).toBe(true);
    
    spGroup.patchValue({
      startDate: '2027-01-01',
      endDate: '2027-01-05' // Valid
    });
    
    expect(spGroup.hasError('dateRange')).toBe(false);
  });

  it('should set available slots to 9999 for Honeymoon package on auto-draft', () => {
    component.packageForm.patchValue({
      title: 'Honeymoon Trip',
      destination: 'Maldives',
      type: 'Honeymoon'
    });
    component.packageForm.markAsDirty();
    
    component.onSubmit('Draft', true);
    
    const formDataArg = packageServiceMock.createPackage.mock.calls[0][0];
    const packageDataStr = formDataArg.get('PackageData');
    const packageData = JSON.parse(packageDataStr);
    
    expect(packageData.seasonalPricing[0].availableSlots).toBe(9999);
  });

  it('should trigger auto-draft to localStorage when form values change', () => {
    vi.useFakeTimers();
    const setItemSpy = vi.spyOn(Storage.prototype, 'setItem');
    
    component.packageForm.patchValue({
      title: 'Auto Draft Test',
      destination: 'Paris'
    });
    component.packageForm.markAsDirty();
    
    vi.advanceTimersByTime(1500); // Wait for debounceTime
    
    expect(setItemSpy).toHaveBeenCalledWith('tourmate_package_draft', expect.any(String));
    vi.useRealTimers();
  });
});
