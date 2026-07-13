import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApplyAgencyComponent } from './apply-agency';
import { AgencyService } from '../../services/agency.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('ApplyAgencyComponent', () => {
  let component: ApplyAgencyComponent;
  let fixture: ComponentFixture<ApplyAgencyComponent>;

  let agencyServiceSpy: any;
  let authServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    agencyServiceSpy = {
      getMyPackagerStatus: vi.fn().mockReturnValue(of({ companyName: 'Test Agency', approvalStatus: 'Pending' })),
      applyToBecomePackager: vi.fn().mockReturnValue(of({}))
    };
    authServiceSpy = {
      getUserRole: vi.fn().mockReturnValue('User')
    };

    await TestBed.configureTestingModule({
      imports: [ApplyAgencyComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AgencyService, useValue: agencyServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ApplyAgencyComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should set admin mode if user is Admin', () => {
    authServiceSpy.getUserRole.mockReturnValue('Admin');
    fixture.detectChanges();
    
    expect(component.isAdmin()).toBeTruthy();
    expect(component.isLoadingStatus()).toBeFalsy();
    expect(agencyServiceSpy.getMyPackagerStatus).not.toHaveBeenCalled();
  });

  it('should load packager status if user is not Admin', () => {
    fixture.detectChanges();
    
    expect(component.isAdmin()).toBeFalsy();
    expect(agencyServiceSpy.getMyPackagerStatus).toHaveBeenCalled();
    expect(component.applicationStatus()).toEqual({ companyName: 'Test Agency', approvalStatus: 'Pending' });
    expect(component.isLoadingStatus()).toBeFalsy();
  });

  it('should handle missing packager status (new applicant)', () => {
    agencyServiceSpy.getMyPackagerStatus.mockReturnValue(throwError(() => new Error('Not found')));
    fixture.detectChanges();
    
    expect(component.applicationStatus()).toBeNull();
    expect(component.isLoadingStatus()).toBeFalsy();
  });

  it('should validate step 2 correctly', () => {
    fixture.detectChanges();
    component.nextStep(2);
    expect(component.errorMessage()).toBe('Company Name is required.');
    
    component.applyForm.patchValue({ companyName: 'Test' });
    component.nextStep(2);
    expect(component.errorMessage()).toBe('A valid Business License Number (min 5 characters) is required.');
    
    component.applyForm.patchValue({ businessLicenseNo: '12345' });
    component.nextStep(2);
    expect(component.errorMessage()).toBe('Please upload all required documents.');
    
    component.panDocument = new File([], 'pan');
    component.gstDocument = new File([], 'gst');
    component.businessRegistration = new File([], 'reg');
    
    component.nextStep(2);
    expect(component.errorMessage()).toBe('');
    expect(component.currentStep()).toBe(2);
  });

  it('should validate step 3 correctly', () => {
    fixture.detectChanges();
    component.nextStep(3);
    expect(component.errorMessage()).toBe('A valid Email is required.');
    
    component.applyForm.patchValue({ contactEmail: 'test@test.com' });
    component.nextStep(3);
    expect(component.errorMessage()).toBe('A valid 10-digit Phone Number is required.');
    
    component.applyForm.patchValue({ contactPhone: '1234567890' });
    component.nextStep(3);
    expect(component.errorMessage()).toBe('A valid Website URL is required.');
    
    component.applyForm.patchValue({ websiteUrl: 'https://example.com' });
    component.nextStep(3);
    expect(component.errorMessage()).toBe('A valid Description (min 20 characters) is required.');
    
    component.applyForm.patchValue({ description: 'This is a description long enough' });
    component.nextStep(3);
    expect(component.errorMessage()).toBe('');
    expect(component.currentStep()).toBe(3);
  });

  it('should handle file selection', () => {
    fixture.detectChanges();
    const file = new File([], 'test.pdf');
    const event = { target: { files: [file] } } as unknown as Event;
    
    component.onFileSelected(event, 'pan');
    expect(component.panDocument).toBe(file);
    
    component.onFileSelected(event, 'gst');
    expect(component.gstDocument).toBe(file);
    
    component.onFileSelected(event, 'registration');
    expect(component.businessRegistration).toBe(file);
  });

  it('should handle empty file selection', () => {
    fixture.detectChanges();
    const event = { target: { files: [] } } as unknown as Event;
    
    component.onFileSelected(event, 'pan');
    expect(component.panDocument).toBeNull();
  });

  it('should prevent submission if invalid', () => {
    fixture.detectChanges();
    component.submitApplication();
    expect(component.errorMessage()).toBe('Please fill out all required fields and upload documents.');
    expect(agencyServiceSpy.applyToBecomePackager).not.toHaveBeenCalled();
  });

  it('should submit application successfully', () => {
    fixture.detectChanges();
    
    component.applyForm.patchValue({
      companyName: 'Test Agency',
      businessLicenseNo: '12345',
      contactEmail: 'test@test.com',
      contactPhone: '1234567890',
      websiteUrl: 'https://example.com',
      description: 'This is a description long enough'
    });
    
    component.panDocument = new File([], 'pan');
    component.gstDocument = new File([], 'gst');
    component.businessRegistration = new File([], 'reg');
    
    component.submitApplication();
    
    expect(agencyServiceSpy.applyToBecomePackager).toHaveBeenCalled();
    expect(component.isSubmitting()).toBeFalsy();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should handle application submission error', () => {
    agencyServiceSpy.applyToBecomePackager.mockReturnValue(throwError(() => ({ error: { message: 'Submission failed' } })));
    fixture.detectChanges();
    
    component.applyForm.patchValue({
      companyName: 'Test Agency',
      businessLicenseNo: '12345',
      contactEmail: 'test@test.com',
      contactPhone: '1234567890',
      websiteUrl: 'https://example.com',
      description: 'This is a description long enough'
    });
    
    component.panDocument = new File([], 'pan');
    component.gstDocument = new File([], 'gst');
    component.businessRegistration = new File([], 'reg');
    
    component.submitApplication();
    
    expect(component.isSubmitting()).toBeFalsy();
    expect(component.errorMessage()).toBe('Submission failed');
  });
});
