import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { environment } from '../../environments/environment';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    // Clear localStorage to prevent automatic loadProfile call on construction
    localStorage.removeItem('jwt_token');

    TestBed.configureTestingModule({
      providers: [
        UserService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load profile and set signal', () => {
    const mockProfile = { id: 'u1', firstName: 'John', lastName: 'Doe', email: 'j@d.com', role: 'Traveler' };

    service.loadProfile().subscribe((profile) => {
      expect(profile).toEqual(mockProfile);
      expect(service.userProfile()).toEqual(mockProfile);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Users/profile`);
    expect(req.request.method).toBe('GET');
    req.flush(mockProfile);
  });

  it('should get profile', () => {
    const mockProfile = { id: 'u1', firstName: 'John', lastName: 'Doe', email: 'j@d.com', role: 'Traveler' };

    service.getProfile().subscribe((profile) => {
      expect(profile).toEqual(mockProfile);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Users/profile`);
    expect(req.request.method).toBe('GET');
    req.flush(mockProfile);
  });

  it('should update profile', () => {
    const updateReq: any = { firstName: 'Jane', fullName: 'Jane Doe' };

    service.updateProfile(updateReq).subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Users/profile`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateReq);
    req.flush({ success: true });
  });

  it('should upload profile picture', () => {
    const file = new File([''], 'pic.jpg', { type: 'image/jpeg' });

    service.uploadProfilePicture(file).subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Users/profile/picture`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });

  it('should remove profile picture', () => {
    service.removeProfilePicture().subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Users/profile/picture`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true });
  });
});
