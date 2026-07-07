import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { UserService } from './user.service';
import { environment } from '../../environments/environment';
import { of } from 'rxjs';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let mockUserService: any;

  beforeEach(() => {
    // Mock user service to prevent it from calling APIs
    mockUserService = {
      loadProfile: vi.fn().mockReturnValue(of({})),
      userProfile: { set: vi.fn() }
    };
    
    // Clear localStorage
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: UserService, useValue: mockUserService },
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should register a user', () => {
    const data = { email: 'test@test.com', password: 'password' };

    service.register(data).subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/register`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });

  it('should login and set tokens', () => {
    const data = { email: 'test@test.com', password: 'password' };
    const mockRes = { token: 'mock-jwt-token', refreshToken: 'mock-refresh-token' };

    service.login(data).subscribe((res: any) => {
      expect(res).toEqual(mockRes);
      expect(localStorage.getItem('jwt_token')).toBe('mock-jwt-token');
      expect(localStorage.getItem('refresh_token')).toBe('mock-refresh-token');
      expect(service.isAuthenticated()).toBe(true);
      expect(mockUserService.loadProfile).toHaveBeenCalled();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockRes);
  });

  it('should clear tokens on logout', () => {
    localStorage.setItem('jwt_token', 'test-token');
    service.isAuthenticated.set(true);

    service.logout();

    expect(localStorage.getItem('jwt_token')).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(mockUserService.userProfile.set).toHaveBeenCalledWith(null);
  });

  it('should parse role from token', () => {
    // mock jwt with role 'Packager'
    // Header: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9 ({"alg":"HS256","typ":"JWT"})
    // Payload: eyJyb2xlIjoiUGFja2FnZXIifQ== ({"role":"Packager"})
    const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiUGFja2FnZXIifQ==.signature';
    localStorage.setItem('jwt_token', token);

    const role = service.getUserRole();
    expect(role).toBe('Packager');
  });
  
  it('should parse email verified from token', () => {
    const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJFbWFpbFZlcmlmaWVkIjoiVHJ1ZSJ9.signature';
    localStorage.setItem('jwt_token', token);

    const verified = service.isEmailVerified();
    expect(verified).toBe(true);
  });
});
