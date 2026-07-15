import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WishlistComponent } from './wishlist';
import { WishlistService } from '../../services/wishlist.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';

describe('WishlistComponent', () => {
  let component: WishlistComponent;
  let fixture: ComponentFixture<WishlistComponent>;

  let wishlistServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    wishlistServiceSpy = {
      getWishlists: vi.fn().mockReturnValue(of({ data: [] })),
      toggleWishlist: vi.fn(),
      wishlistedPackageIds: signal(new Set<string>())
    };

    await TestBed.configureTestingModule({
      imports: [WishlistComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: WishlistService, useValue: wishlistServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WishlistComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create and load wishlists on init', () => {
    wishlistServiceSpy.getWishlists.mockReturnValue(of({
      data: [{ packageId: 'pkg-1', package: { title: 'Pkg 1' } }]
    }));
    
    fixture.detectChanges();
    
    expect(component.wishlists().length).toBe(1);
    expect(component.wishlists()[0].packageId).toBe('pkg-1');
    expect(wishlistServiceSpy.wishlistedPackageIds().has('pkg-1')).toBeTruthy();
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle error when loading wishlists', () => {
    wishlistServiceSpy.getWishlists.mockReturnValue(throwError(() => new Error('error')));
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Failed to load wishlist. Please try again.');
    expect(component.isLoading()).toBeFalsy();
    expect(component.wishlists().length).toBe(0);
  });

  it('should toggle wishlist and optimistically update local state', () => {
    wishlistServiceSpy.getWishlists.mockReturnValue(of({
      data: [
        { packageId: 'pkg-1', package: { title: 'Pkg 1' } },
        { packageId: 'pkg-2', package: { title: 'Pkg 2' } }
      ]
    }));
    
    fixture.detectChanges();
    expect(component.wishlists().length).toBe(2);
    
    component.toggleWishlist('pkg-1', 'Pkg 1');
    
    expect(wishlistServiceSpy.toggleWishlist).toHaveBeenCalledWith('pkg-1', 'Pkg 1');
    expect(component.wishlists().length).toBe(1);
    expect(component.wishlists()[0].packageId).toBe('pkg-2');
  });

  it('should navigate to view details', () => {
    fixture.detectChanges();
    component.viewDetails('pkg-1');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/package', 'pkg-1']);
  });

  it('should get correct primary image', () => {
    expect(component.getPrimaryImage({})).toContain('aida-public');
    expect(component.getPrimaryImage({ primaryImageUrl: 'http://img.jpg' })).toBe('http://img.jpg');
    expect(component.getPrimaryImage({ primaryImageUrl: '/img.jpg' })).toBe(`${environment.baseUrl}/img.jpg`);
  });

  it('should get correct starting price', () => {
    expect(component.getStartingPrice({ startingPrice: 500 })).toBe(500);
    expect(component.getStartingPrice({})).toBe(0);
  });
});
