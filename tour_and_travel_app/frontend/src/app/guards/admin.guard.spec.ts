import { TestBed } from '@angular/core/testing';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let authServiceSpy: any;
  let routerSpy: any;

  beforeEach(() => {
    authServiceSpy = {
      isAuthenticated: vi.fn(),
      getUserRole: vi.fn()
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

  const runGuard = () => {
    return TestBed.runInInjectionContext(() => {
      return adminGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot);
    });
  };

  it('should allow access if authenticated and role is Admin', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.getUserRole.mockReturnValue('Admin');

    const result = runGuard();

    expect(result).toBe(true);
    expect(routerSpy.parseUrl).not.toHaveBeenCalled();
  });

  it('should redirect to home if not authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(false);
    authServiceSpy.getUserRole.mockReturnValue(null);
    const mockUrlTree = {};
    routerSpy.parseUrl.mockReturnValue(mockUrlTree);

    const result = runGuard();

    expect(result).toBe(mockUrlTree);
    expect(routerSpy.parseUrl).toHaveBeenCalledWith('/');
  });

  it('should redirect to home if authenticated but role is not Admin', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.getUserRole.mockReturnValue('User');
    const mockUrlTree = {};
    routerSpy.parseUrl.mockReturnValue(mockUrlTree);

    const result = runGuard();

    expect(result).toBe(mockUrlTree);
    expect(routerSpy.parseUrl).toHaveBeenCalledWith('/');
  });
});
