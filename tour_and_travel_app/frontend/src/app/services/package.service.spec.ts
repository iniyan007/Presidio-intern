import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PackageService } from './package.service';
import { environment } from '../../environments/environment';

describe('PackageService', () => {
  let service: PackageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PackageService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(PackageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch packages with filters', () => {
    const mockPackages = { items: [{ id: '1', title: 'Test Package' }] };
    const filters = { SearchTerm: 'Test', MinPrice: 100, PageNumber: 1 };

    service.getPackages(filters).subscribe((res) => {
      expect(res).toEqual(mockPackages);
    });

    const req = httpMock.expectOne((request) => 
      request.url === `${environment.apiUrl}/Packages` && 
      request.params.get('SearchTerm') === 'Test' && 
      request.params.get('MinPrice') === '100' && 
      request.params.get('PageNumber') === '1'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockPackages);
  });

  it('should fetch package by id', () => {
    const mockPackage = { id: 'p1', title: 'Package 1' };

    service.getPackageById('p1').subscribe((res: any) => {
      expect(res).toEqual(mockPackage);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages/p1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPackage);
  });

  it('should fetch package reviews', () => {
    const mockReviews = [{ id: 'r1', comment: 'Great!' }];

    service.getPackageReviews('p1').subscribe((res: any) => {
      expect(res).toEqual(mockReviews);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages/p1/reviews`);
    expect(req.request.method).toBe('GET');
    req.flush(mockReviews);
  });

  it('should create a package', () => {
    const formData = new FormData();
    formData.append('title', 'New Package');
    
    service.createPackage(formData).subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });

  it('should fetch my packages', () => {
    const mockPackages = [{ id: 'm1', title: 'My Package' }];

    service.getMyPackages().subscribe((res: any) => {
      expect(res).toEqual(mockPackages);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages/me`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPackages);
  });

  it('should fetch my package by id', () => {
    const mockPackage = { id: 'm1', title: 'My Package Details' };

    service.getMyPackageById('m1').subscribe((res: any) => {
      expect(res).toEqual(mockPackage);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages/me/m1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPackage);
  });

  it('should update full package', () => {
    const formData = new FormData();
    
    service.updateFullPackage('m1', formData).subscribe((res: any) => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packages/me/m1`);
    expect(req.request.method).toBe('PUT');
    req.flush({ success: true });
  });
});
