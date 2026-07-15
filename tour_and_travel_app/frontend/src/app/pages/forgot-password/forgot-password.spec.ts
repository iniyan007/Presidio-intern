import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ForgotPasswordComponent } from './forgot-password';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('ForgotPasswordComponent', () => {
  let component: ForgotPasswordComponent;
  let fixture: ComponentFixture<ForgotPasswordComponent>;

  let authServiceSpy: any;
  let toastServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    authServiceSpy = {
      forgotPassword: vi.fn().mockReturnValue(of({})),
      verifyResetOtp: vi.fn().mockReturnValue(of({ resetToken: 'fake-token' })),
      resetPassword: vi.fn().mockReturnValue(of({}))
    };
    
    toastServiceSpy = {
      show: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent, ReactiveFormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ForgotPasswordComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should lowercase email on value changes', () => {
    fixture.detectChanges();
    component.emailForm.controls['email'].setValue('Test@EMAIL.com');
    expect(component.emailForm.controls['email'].value).toBe('test@email.com');
  });

  it('should not request OTP if form is invalid', () => {
    fixture.detectChanges();
    component.emailForm.controls['email'].setValue('invalid');
    component.requestOtp();
    expect(authServiceSpy.forgotPassword).not.toHaveBeenCalled();
  });

  it('should request OTP successfully', () => {
    fixture.detectChanges();
    component.emailForm.controls['email'].setValue('test@test.com');
    component.requestOtp();
    
    expect(authServiceSpy.forgotPassword).toHaveBeenCalledWith('test@test.com');
    expect(component.step()).toBe(2);
    expect(component.email()).toBe('test@test.com');
    expect(toastServiceSpy.show).toHaveBeenCalledWith('OTP sent to your email', 'success');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle request OTP error', () => {
    fixture.detectChanges();
    authServiceSpy.forgotPassword.mockReturnValue(throwError(() => ({ error: { message: 'Not found' } })));
    component.emailForm.controls['email'].setValue('test@test.com');
    component.requestOtp();
    
    expect(component.errorMessage()).toBe('Not found');
    expect(component.step()).toBe(1);
    expect(component.isLoading()).toBeFalsy();
  });

  it('should not verify OTP if form is invalid', () => {
    fixture.detectChanges();
    component.otpForm.controls['otp'].setValue('123'); // too short
    component.verifyOtp();
    expect(authServiceSpy.verifyResetOtp).not.toHaveBeenCalled();
  });

  it('should verify OTP successfully', () => {
    fixture.detectChanges();
    component.email.set('test@test.com');
    component.otpForm.controls['otp'].setValue('123456');
    component.verifyOtp();
    
    expect(authServiceSpy.verifyResetOtp).toHaveBeenCalledWith('test@test.com', '123456');
    expect(component.step()).toBe(3);
    expect(component.resetToken()).toBe('fake-token');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle verify OTP error', () => {
    fixture.detectChanges();
    authServiceSpy.verifyResetOtp.mockReturnValue(throwError(() => ({ error: { message: 'Invalid OTP' } })));
    component.email.set('test@test.com');
    component.otpForm.controls['otp'].setValue('123456');
    component.verifyOtp();
    
    expect(component.errorMessage()).toBe('Invalid OTP');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should not reset password if form is invalid', () => {
    fixture.detectChanges();
    component.passwordForm.controls['newPassword'].setValue('short');
    component.passwordForm.controls['confirmPassword'].setValue('short');
    component.resetPassword();
    expect(authServiceSpy.resetPassword).not.toHaveBeenCalled();
  });

  it('should not reset password if passwords do not match', () => {
    fixture.detectChanges();
    component.passwordForm.controls['newPassword'].setValue('password123');
    component.passwordForm.controls['confirmPassword'].setValue('password456');
    component.resetPassword();
    
    expect(component.errorMessage()).toBe('Passwords do not match.');
    expect(authServiceSpy.resetPassword).not.toHaveBeenCalled();
  });

  it('should reset password successfully', () => {
    fixture.detectChanges();
    component.email.set('test@test.com');
    component.resetToken.set('token');
    component.passwordForm.controls['newPassword'].setValue('password123');
    component.passwordForm.controls['confirmPassword'].setValue('password123');
    
    component.resetPassword();
    
    expect(authServiceSpy.resetPassword).toHaveBeenCalledWith({
      email: 'test@test.com',
      resetToken: 'token',
      newPassword: 'password123'
    });
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Password reset successfully!', 'success');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth'], { queryParams: { tab: 'login' } });
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle reset password error', () => {
    fixture.detectChanges();
    authServiceSpy.resetPassword.mockReturnValue(throwError(() => ({ error: { message: 'Failed reset' } })));
    component.email.set('test@test.com');
    component.resetToken.set('token');
    component.passwordForm.controls['newPassword'].setValue('password123');
    component.passwordForm.controls['confirmPassword'].setValue('password123');
    
    component.resetPassword();
    
    expect(component.errorMessage()).toBe('Failed reset');
    expect(component.isLoading()).toBeFalsy();
  });
});
