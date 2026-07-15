import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AdminService } from './admin.service';
import { environment } from '../../environments/environment';

describe('AdminService', () => {
  let service: AdminService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AdminService]
    });
    service = TestBed.inject(AdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get pending packagers with params', () => {
    service.getPendingPackagers('test', 'name_asc').subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/pending?search=test&sort=name_asc`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get approved packagers with params', () => {
    service.getApprovedPackagers('test', 'name_asc').subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/approved?search=test&sort=name_asc`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get deactivated packagers with params', () => {
    service.getDeactivatedPackagers('test', 'name_asc').subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/deactivated?search=test&sort=name_asc`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get packager documents', () => {
    const packagerId = '123';
    service.getPackagerDocuments(packagerId).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/${packagerId}/documents`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should approve packager', () => {
    const packagerId = '123';
    service.approvePackager(packagerId).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/${packagerId}/approve`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should reject packager', () => {
    const packagerId = '123';
    const reason = 'Invalid documents';
    
    service.rejectPackager(packagerId, reason).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/${packagerId}/reject`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ reason });
    req.flush({});
  });

  it('should deactivate packager', () => {
    const packagerId = '123';
    const reason = 'Violation of terms';
    
    service.deactivatePackager(packagerId, reason).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/${packagerId}/deactivate`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ reason });
    req.flush({});
  });

  it('should activate packager', () => {
    const packagerId = '123';
    service.activatePackager(packagerId).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/packagers/${packagerId}/activate`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should get analytics', () => {
    service.getAnalytics().subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Admin/analytics`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get platform config', () => {
    service.getPlatformConfig().subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/PlatformConfig`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should update platform config', () => {
    service.updatePlatformConfig(10, 18).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/PlatformConfig`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ platformFeePercent: 10, gstPercent: 18 });
    req.flush({});
  });
});
