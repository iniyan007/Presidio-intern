import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MetadataService, MetadataEnums } from './metadata.service';
import { environment } from '../../environments/environment';

describe('MetadataService', () => {
  let service: MetadataService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MetadataService]
    });
    service = TestBed.inject(MetadataService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get enums', () => {
    const mockResponse: MetadataEnums = {
      packageTypes: ['type1'],
      packageStatuses: ['status1'],
      inclusionTypes: ['inc1'],
      mediaCategories: ['cat1'],
      mealTypes: ['meal1'],
      transportModes: ['trans1'],
      daySessions: ['sess1'],
      bookingStatuses: ['bk1'],
      paymentStatuses: ['pay1']
    };

    service.getEnums().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Metadata/enums`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get countries', () => {
    const mockResponse: string[] = ['Country1', 'Country2'];

    service.getCountries().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Metadata/countries`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get destinations without country', () => {
    const mockResponse: string[] = ['Dest1', 'Dest2'];

    service.getDestinations().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Metadata/destinations`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get destinations with country parameter', () => {
    const mockResponse: string[] = ['Dest1'];
    const country = 'TestCountry';

    service.getDestinations(country).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Metadata/destinations?country=${country}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
