import { TestBed } from '@angular/core/testing';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { guestGuard } from './guest.guard';
import { AuthService } from '../services/auth.service';

describe('guestGuard', () => {
  let authServiceSpy: any;
  let routerSpy: any;

  beforeEach(() => {
    authServiceSpy = {
      isAuthenticated: vi.fn(),
      isEmailVerified: vi.fn()
    };
    routerSpy = {
      parseUrl: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
  });

  const runGuard = (url: string = '/') => {
    return TestBed.runInInjectionContext(() => {
      return guestGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot);
    });
  };

  it('should allow access if not authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(false);

    const result = runGuard();

    expect(result).toBe(true);
    expect(routerSpy.parseUrl).not.toHaveBeenCalled();
  });

  it('should redirect to verify-email if authenticated but email not verified and trying to access other pages', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.isEmailVerified.mockReturnValue(false);
    const mockUrlTree = {};
    routerSpy.parseUrl.mockReturnValue(mockUrlTree);

    const result = runGuard('/login');

    expect(result).toBe(mockUrlTree);
    expect(routerSpy.parseUrl).toHaveBeenCalledWith('/verify-email');
  });

  it('should allow access if authenticated and email not verified and trying to access /verify-email', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.isEmailVerified.mockReturnValue(false);

    const result = runGuard('/verify-email');

    expect(result).toBe(true);
    expect(routerSpy.parseUrl).not.toHaveBeenCalled();
  });

  it('should redirect to home if authenticated and email is verified', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.isEmailVerified.mockReturnValue(true);
    const mockUrlTree = {};
    routerSpy.parseUrl.mockReturnValue(mockUrlTree);

    const result = runGuard('/login');

    expect(result).toBe(mockUrlTree);
    expect(routerSpy.parseUrl).toHaveBeenCalledWith('/');
  });
});
