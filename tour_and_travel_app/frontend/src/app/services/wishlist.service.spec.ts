import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WishlistService } from './wishlist.service';
import { ToastService } from './toast.service';
import { environment } from '../../environments/environment';
import { vi } from 'vitest';

describe('WishlistService', () => {
  let service: WishlistService;
  let httpMock: HttpTestingController;
  let toastServiceSpy: any;

  beforeEach(() => {
    toastServiceSpy = { show: vi.fn() };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        WishlistService,
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    });
    service = TestBed.inject(WishlistService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load wishlists and update signals', () => {
    const mockResponse = {
      success: true,
      data: [{ packageId: 'pkg1' }, { packageId: 'pkg2' }]
    };

    service.loadWishlists();

    const req = httpMock.expectOne(`${environment.apiUrl}/Wishlists`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);

    expect(service.wishlistedPackageIds().size).toBe(2);
    expect(service.wishlistedPackageIds().has('pkg1')).toBeTruthy();
    expect(service.wishlistedPackageIds().has('pkg2')).toBeTruthy();
  });

  it('should clear wishlists signal', () => {
    service.wishlistedPackageIds.set(new Set(['pkg1']));
    service.clearWishlists();
    expect(service.wishlistedPackageIds().size).toBe(0);
  });

  it('should get wishlists as observable', () => {
    const mockResponse = {
      success: true,
      data: [{ packageId: 'pkg1' }]
    };

    service.getWishlists().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Wishlists`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should toggle wishlist (add)', () => {
    const mockResponse = { success: true, message: 'Added', added: true };

    service.toggleWishlist('pkg1', 'Test Package');

    const req = httpMock.expectOne(`${environment.apiUrl}/Wishlists/toggle/pkg1`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.wishlistedPackageIds().has('pkg1')).toBeTruthy();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('"Test Package" added to wishlist', 'success');
  });

  it('should toggle wishlist (remove)', () => {
    service.wishlistedPackageIds.set(new Set(['pkg1']));
    const mockResponse = { success: true, message: 'Removed', added: false };

    service.toggleWishlist('pkg1', 'Test Package');

    const req = httpMock.expectOne(`${environment.apiUrl}/Wishlists/toggle/pkg1`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.wishlistedPackageIds().has('pkg1')).toBeFalsy();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('"Test Package" removed from wishlist', 'success');
  });

  it('should handle toggle error', () => {
    service.toggleWishlist('pkg1');

    const req = httpMock.expectOne(`${environment.apiUrl}/Wishlists/toggle/pkg1`);
    req.flush({ message: 'Error occurred' }, { status: 400, statusText: 'Bad Request' });

    expect(toastServiceSpy.show).toHaveBeenCalledWith('Error occurred', 'error');
  });
});
