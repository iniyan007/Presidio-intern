import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LocationService } from './location.service';

describe('LocationService', () => {
  let service: LocationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LocationService]
    });
    service = TestBed.inject(LocationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return empty array for empty query', () => {
    service.searchLocation('').subscribe(res => {
      expect(res).toEqual([]);
    });
    
    // Ensure no HTTP request was made
    httpMock.expectNone('https://nominatim.openstreetmap.org/search');
  });

  it('should search location and map results', () => {
    const mockResponse = [
      {
        display_name: 'Paris, France',
        name: 'Paris',
        address: { city: 'Paris', country: 'France' },
        extra_field: 'ignored'
      }
    ];

    service.searchLocation('Paris').subscribe(res => {
      expect(res.length).toBe(1);
      expect(res[0].display_name).toBe('Paris, France');
      expect(res[0].name).toBe('Paris');
      expect(res[0].address.city).toBe('Paris');
      // Should not include ignored extra fields
      expect((res[0] as any).extra_field).toBeUndefined();
    });

    const req = httpMock.expectOne(request => 
      request.url === 'https://nominatim.openstreetmap.org/search' &&
      request.params.get('q') === 'Paris' &&
      request.params.get('format') === 'json'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should handle errors gracefully and return empty array', () => {
    service.searchLocation('ErrorCity').subscribe(res => {
      expect(res).toEqual([]);
    });

    const req = httpMock.expectOne(request => request.url === 'https://nominatim.openstreetmap.org/search');
    req.flush('Error', { status: 500, statusText: 'Server Error' });
  });
});
