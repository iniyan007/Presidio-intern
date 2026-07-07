import { vi } from 'vitest';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

import { packagerGuard } from './packager.guard';

describe('packagerGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => packagerGuard(...guardParameters));
    
  let authServiceMock: any;
  let routerMock: any;

  beforeEach(() => {
    authServiceMock = {
      isAuthenticated: vi.fn().mockReturnValue(true),
      getUserRole: vi.fn().mockReturnValue('Packager')
    };
    
    routerMock = {
      parseUrl: vi.fn().mockReturnValue('mockUrl')
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]), 
        provideHttpClient(), 
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  it('should allow navigation if user is authenticated Packager', () => {
    const result = executeGuard({} as any, {} as any);
    expect(result).toBe(true);
  });

  it('should redirect to root if not authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);
    
    const result = executeGuard({} as any, {} as any);
    expect(result).toBe('mockUrl');
    expect(routerMock.parseUrl).toHaveBeenCalledWith('/');
  });

  it('should redirect to root if authenticated but not Packager', () => {
    authServiceMock.getUserRole.mockReturnValue('Traveler');
    
    const result = executeGuard({} as any, {} as any);
    expect(result).toBe('mockUrl');
    expect(routerMock.parseUrl).toHaveBeenCalledWith('/');
  });
});
