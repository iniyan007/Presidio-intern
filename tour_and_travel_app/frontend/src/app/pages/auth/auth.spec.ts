import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { AuthComponent } from './auth';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

describe('AuthComponent', () => {
  let component: AuthComponent;
  let fixture: ComponentFixture<AuthComponent>;
  let authServiceMock: any;
  let toastServiceMock: any;
  let routerMock: any;

  beforeEach(async () => {
    authServiceMock = {
      login: vi.fn().mockReturnValue(of({ success: true })),
      register: vi.fn().mockReturnValue(of({ success: true })),
      isEmailVerified: vi.fn().mockReturnValue(true),
      getUserRole: vi.fn().mockReturnValue('Traveler'),
      sendOtp: vi.fn().mockReturnValue(of({ success: true }))
    };

    toastServiceMock = {
      show: vi.fn()
    };

    routerMock = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AuthComponent],
      providers: [
        provideRouter([]), 
        provideHttpClient(), 
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { queryParams: of({ returnUrl: '/dashboard' }) } }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should validate form and not call API if invalid', () => {
    // Empty form
    component.onLogin();
    expect(authServiceMock.login).not.toHaveBeenCalled();
    
    component.onSignup();
    expect(authServiceMock.register).not.toHaveBeenCalled();
  });

  it('should call login when valid login data is submitted', () => {
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: 'Password123!'
    });
    
    component.onLogin();
    
    expect(authServiceMock.login).toHaveBeenCalled();
    expect(toastServiceMock.show).toHaveBeenCalledWith('Logged in successfully', 'success');
  });

  it('should call register when valid signup data is submitted', () => {
    component.signupForm.patchValue({
      fullName: 'John Doe',
      email: 'test@example.com',
      password: 'Password123!',
      phone: '9876543210'
    });
    
    component.onSignup();
    
    expect(authServiceMock.register).toHaveBeenCalled();
    expect(toastServiceMock.show).toHaveBeenCalledWith('Registration successful! Please login.', 'success');
  });

  it('should handle login error', () => {
    authServiceMock.login.mockReturnValue(throwError(() => ({ error: { message: 'Invalid credentials' }, status: 401 })));
    
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: 'WrongPassword!'
    });
    
    component.onLogin();
    
    expect(authServiceMock.login).toHaveBeenCalled();
    expect(toastServiceMock.show).toHaveBeenCalledWith('Invalid email or password. Please try again.', 'error');
    expect(component.isLoading()).toBe(false);
  });
});
