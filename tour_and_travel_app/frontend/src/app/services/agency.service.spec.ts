import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AgencyService } from './agency.service';
import { environment } from '../../environments/environment';

describe('AgencyService', () => {
  let service: AgencyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AgencyService]
    });
    service = TestBed.inject(AgencyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should search public packagers', () => {
    const mockResponse = [{ id: '1', agencyName: 'Test Agency' }];

    service.searchPublicPackagers('test').subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packagers/public?SearchTerm=test`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get public packager by name', () => {
    const mockResponse: any = { id: '1', agencyName: 'Test Agency' };

    service.getPublicPackagerByName('test-agency').subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packagers/public/test-agency`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get packager reviews', () => {
    const packagerId = '123';
    const mockResponse: any[] = [{ id: 'rev1', rating: 5 }];

    service.getPackagerReviews(packagerId).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packagers/${packagerId}/reviews`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should apply to become packager', () => {
    const formData = new FormData();
    formData.append('agencyName', 'New Agency');
    const mockResponse = { success: true };

    service.applyToBecomePackager(formData).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packagers/apply`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBe(formData);
    req.flush(mockResponse);
  });

  it('should get my packager status', () => {
    const mockResponse = { status: 'Pending' };

    service.getMyPackagerStatus().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Packagers/me/status`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
