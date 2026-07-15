import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BookingService } from './booking.service';
import { environment } from '../../environments/environment';

describe('BookingService', () => {
  let service: BookingService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookingService]
    });
    service = TestBed.inject(BookingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get platform config', () => {
    const mockResponse: any = { adminFeePercentage: 5 };

    service.getPlatformConfig().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/PlatformConfig`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should create booking', () => {
    const formData = new FormData();
    const mockResponse: any = { id: 'booking1' };

    service.createBooking(formData).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBe(formData);
    req.flush(mockResponse);
  });

  it('should get my bookings', () => {
    const mockResponse: any[] = [{ id: 'booking1' }];

    service.getMyBookings().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/my-bookings`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get bookings by package id', () => {
    const packageId = 'pkg1';
    const mockResponse: any[] = [{ id: 'booking1' }];

    service.getBookingsByPackageId(packageId).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/package/${packageId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should cancel booking', () => {
    const id = 'booking1';
    const request: any = { reason: 'test' };

    service.cancelBooking(id, request).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/${id}/cancel`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('should process payment', () => {
    const id = 'booking1';
    const request: any = { amount: 100 };

    service.processPayment(id, request).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/${id}/pay`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('should download ticket', () => {
    const id = 'booking1';
    const mockBlob = new Blob(['ticket content'], { type: 'application/pdf' });

    service.downloadTicket(id).subscribe(res => {
      expect(res).toEqual(mockBlob);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/${id}/ticket`);
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(mockBlob);
  });

  it('should reupload document', () => {
    const docId = 'doc1';
    const file = new File(['content'], 'test.pdf');

    service.reuploadDocument(docId, file).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/documents/${docId}/reupload`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should create review', () => {
    const bookingId = 'booking1';
    const reviewData = { rating: 5, comment: 'Great' };

    service.createReview(bookingId, reviewData).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/${bookingId}/reviews`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reviewData);
    req.flush({});
  });

  it('should upload review media', () => {
    const files = [new File([''], 'test1.jpg')];
    const mockResponse = { success: true, paths: ['/path/to/media'] };

    service.uploadReviewMedia(files).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const baseUrl = `${environment.apiUrl}/Bookings`.replace('/api/Bookings', '');
    const req = httpMock.expectOne(`${baseUrl}/api/Reviews/upload-media`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should verify document', () => {
    const docId = 'doc1';
    const request: any = { isVerified: true };

    service.verifyDocument(docId, request).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/documents/${docId}/verify`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('should verify booking', () => {
    const id = 'booking1';

    service.verifyBooking(id).subscribe(res => {
      expect(res).toBeDefined();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Bookings/${id}/verify`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });
});
