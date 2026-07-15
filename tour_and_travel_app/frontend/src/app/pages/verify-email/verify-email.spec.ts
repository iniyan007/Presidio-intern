import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VerifyEmailComponent } from './verify-email';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('VerifyEmailComponent', () => {
  let component: VerifyEmailComponent;
  let fixture: ComponentFixture<VerifyEmailComponent>;

  let authServiceSpy: any;
  let userServiceSpy: any;
  let toastServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    vi.useFakeTimers();

    authServiceSpy = {
      sendOtp: vi.fn().mockReturnValue(of({})),
      verifyOtp: vi.fn().mockReturnValue(of({}))
    };
    
    userServiceSpy = {
      userProfile: signal(null),
      loadProfile: vi.fn().mockReturnValue(of({ email: 'testuser@example.com' }))
    };
    
    toastServiceSpy = {
      show: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [VerifyEmailComponent, ReactiveFormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VerifyEmailComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
    
    // Mock otpInputs
    component.otpInputs = {
      toArray: () => [
        { nativeElement: { focus: vi.fn() } },
        { nativeElement: { focus: vi.fn() } },
        { nativeElement: { focus: vi.fn() } },
        { nativeElement: { focus: vi.fn() } },
        { nativeElement: { focus: vi.fn() } },
        { nativeElement: { focus: vi.fn() } }
      ]
    } as any;
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('should initialize and load masked email from profile signal', () => {
    userServiceSpy.userProfile.set({ email: 'hello@example.com' });
    fixture.detectChanges();
    
    expect(component.maskedEmail()).toBe('hel***@example.com');
  });

  it('should initialize and load masked email from loadProfile if signal is null', () => {
    fixture.detectChanges();
    
    expect(userServiceSpy.loadProfile).toHaveBeenCalled();
    expect(component.maskedEmail()).toBe('tes***@example.com');
  });

  it('should mask email correctly', () => {
    expect(component.maskEmailString('me@example.com')).toBe('me***@example.com');
    expect(component.maskEmailString('user@test.com')).toBe('use***@test.com');
    expect(component.maskEmailString('invalidemail')).toBe('invalidemail');
  });

  it('should run timer and stop at 0', () => {
    fixture.detectChanges();
    expect(component.timeLeft()).toBe(50);
    
    vi.advanceTimersByTime(50000); // 50 seconds
    
    expect(component.timeLeft()).toBe(0);
    
    vi.advanceTimersByTime(2000); // Wait more
    expect(component.timeLeft()).toBe(0); // Should not go below 0
  });

  it('should handle onInput moving to next field', () => {
    fixture.detectChanges();
    const event = { target: { value: '5' } };
    
    const nextInput = component.otpInputs.toArray()[1].nativeElement;
    const focusSpy = vi.spyOn(nextInput, 'focus');
    
    component.onInput(event, 0);
    expect(focusSpy).toHaveBeenCalled();
  });

  it('should handle onInput with invalid char', () => {
    fixture.detectChanges();
    const event = { target: { value: 'a' } };
    
    component.onInput(event, 0);
    expect(component.otpForm.value.otp[0]).toBe('');
  });

  it('should handle onKeyDown moving to prev field on backspace', () => {
    fixture.detectChanges();
    // Assuming empty input
    const event = { key: 'Backspace' } as KeyboardEvent;
    
    const prevInput = component.otpInputs.toArray()[0].nativeElement;
    const focusSpy = vi.spyOn(prevInput, 'focus');
    
    component.onKeyDown(event, 1);
    expect(focusSpy).toHaveBeenCalled();
  });

  it('should not resend OTP if timer is running', () => {
    fixture.detectChanges();
    component.resendOtp();
    expect(authServiceSpy.sendOtp).not.toHaveBeenCalled();
  });

  it('should resend OTP successfully', () => {
    fixture.detectChanges();
    component.timeLeft.set(0);
    
    component.resendOtp();
    
    expect(authServiceSpy.sendOtp).toHaveBeenCalled();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('OTP sent successfully', 'success');
    expect(component.timeLeft()).toBe(50); // timer restarted
  });

  it('should handle resend OTP error', () => {
    fixture.detectChanges();
    component.timeLeft.set(0);
    authServiceSpy.sendOtp.mockReturnValue(throwError(() => new Error('Error')));
    
    component.resendOtp();
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Failed to send OTP', 'error');
    expect(component.isResending()).toBeFalsy();
  });

  it('should not verify if form is invalid', () => {
    fixture.detectChanges();
    component.verifyOtp();
    expect(authServiceSpy.verifyOtp).not.toHaveBeenCalled();
  });

  it('should verify OTP successfully and navigate after timeout', () => {
    fixture.detectChanges();
    component.otpForm.setValue({ otp: ['1', '2', '3', '4', '5', '6'] });
    
    component.verifyOtp();
    
    expect(authServiceSpy.verifyOtp).toHaveBeenCalledWith('123456');
    expect(component.isSuccess()).toBeTruthy();
    
    vi.advanceTimersByTime(2500);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should handle verify OTP error', () => {
    fixture.detectChanges();
    authServiceSpy.verifyOtp.mockReturnValue(throwError(() => ({ error: { message: 'Invalid OTP' } })));
    component.otpForm.setValue({ otp: ['1', '2', '3', '4', '5', '6'] });
    
    const firstInput = component.otpInputs.toArray()[0].nativeElement;
    const focusSpy = vi.spyOn(firstInput, 'focus');
    
    component.verifyOtp();
    
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Invalid OTP', 'error');
    expect(component.otpForm.value.otp).toEqual([null, null, null, null, null, null]);
    expect(focusSpy).toHaveBeenCalled();
  });

  it('should format time correctly', () => {
    fixture.detectChanges();
    component.timeLeft.set(45);
    expect(component.formatTime()).toBe('00:45');
    component.timeLeft.set(5);
    expect(component.formatTime()).toBe('00:05');
  });
});
