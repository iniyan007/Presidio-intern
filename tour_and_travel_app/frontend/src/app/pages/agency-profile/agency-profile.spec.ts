import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AgencyProfileComponent } from './agency-profile';
import { AgencyService } from '../../services/agency.service';
import { PackageService } from '../../services/package.service';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';

describe('AgencyProfileComponent', () => {
  let component: AgencyProfileComponent;
  let fixture: ComponentFixture<AgencyProfileComponent>;

  let packagerServiceSpy: any;
  let packageServiceSpy: any;
  let routerSpy: any;
  let routeParamMap: BehaviorSubject<any>;
  let routeQueryParamMap: BehaviorSubject<any>;

  beforeEach(async () => {
    packagerServiceSpy = {
      searchPublicPackagers: vi.fn().mockReturnValue(of({ items: [{ id: '1', companyName: 'Test Agency' }] })),
      getPackagerReviews: vi.fn().mockReturnValue(of([]))
    };
    
    packageServiceSpy = {
      getPackageById: vi.fn().mockReturnValue(of({ packagerName: 'Test Agency' })),
      getPackages: vi.fn().mockReturnValue(of({ items: [] }))
    };

    routeParamMap = new BehaviorSubject({ get: () => null });
    routeQueryParamMap = new BehaviorSubject({ get: () => null });

    await TestBed.configureTestingModule({
      imports: [AgencyProfileComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AgencyService, useValue: packagerServiceSpy },
        { provide: PackageService, useValue: packageServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: routeParamMap.asObservable(),
            queryParamMap: routeQueryParamMap.asObservable()
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AgencyProfileComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create and handle invalid profile link', () => {
    routeParamMap.next({ get: () => null });
    routeQueryParamMap.next({ get: () => null });
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Invalid profile link.');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should load profile by name query param', () => {
    routeParamMap.next({ get: () => null });
    routeQueryParamMap.next({ get: (key: string) => key === 'name' ? 'Test Agency' : null });
    
    fixture.detectChanges();
    
    expect(packagerServiceSpy.searchPublicPackagers).toHaveBeenCalledWith('Test Agency');
    expect(component.packager()?.companyName).toBe('Test Agency');
  });

  it('should load profile by packageId route param', () => {
    routeParamMap.next({ get: (key: string) => key === 'packageId' ? 'pkg-1' : null });
    routeQueryParamMap.next({ get: () => null });
    
    fixture.detectChanges();
    
    expect(packageServiceSpy.getPackageById).toHaveBeenCalledWith('pkg-1');
    expect(packagerServiceSpy.searchPublicPackagers).toHaveBeenCalledWith('Test Agency');
  });

  it('should ignore packageId if it is "view" and no name is provided', () => {
    routeParamMap.next({ get: (key: string) => key === 'packageId' ? 'view' : null });
    routeQueryParamMap.next({ get: () => null });
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Invalid profile link.');
  });

  it('should handle package fetch error', () => {
    packageServiceSpy.getPackageById.mockReturnValue(throwError(() => new Error('error')));
    routeParamMap.next({ get: (key: string) => key === 'packageId' ? 'pkg-1' : null });
    routeQueryParamMap.next({ get: () => null });
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Could not load package to find packager.');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle empty packager search result', () => {
    packagerServiceSpy.searchPublicPackagers.mockReturnValue(of({ items: [] }));
    routeQueryParamMap.next({ get: (key: string) => key === 'name' ? 'Unknown' : null });
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Could not find packager details.');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle packager search error', () => {
    packagerServiceSpy.searchPublicPackagers.mockReturnValue(throwError(() => new Error('error')));
    routeQueryParamMap.next({ get: (key: string) => key === 'name' ? 'Error' : null });
    
    fixture.detectChanges();
    
    expect(component.errorMessage()).toBe('Could not search packager details.');
    expect(component.isLoading()).toBeFalsy();
  });

  it('should fetch packages and reviews successfully', () => {
    packagerServiceSpy.searchPublicPackagers.mockReturnValue(of({ data: [{ id: '1', companyName: 'Test Agency' }] }));
    packageServiceSpy.getPackages.mockReturnValue(of({ data: [{ id: 'p1' }] }));
    packagerServiceSpy.getPackagerReviews.mockReturnValue(of([{ rating: 5 }]));
    
    routeQueryParamMap.next({ get: (key: string) => key === 'name' ? 'Test Agency' : null });
    
    fixture.detectChanges();
    
    expect(packageServiceSpy.getPackages).toHaveBeenCalledWith({ PackagerName: 'Test Agency' });
    expect(packagerServiceSpy.getPackagerReviews).toHaveBeenCalledWith('1');
    expect(component.packages().length).toBe(1);
    expect(component.reviews().length).toBe(1);
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle packages and reviews error gracefully', () => {
    packagerServiceSpy.searchPublicPackagers.mockReturnValue(of({ items: [{ id: '1', companyName: 'Test Agency' }] }));
    packageServiceSpy.getPackages.mockReturnValue(throwError(() => new Error('error')));
    packagerServiceSpy.getPackagerReviews.mockReturnValue(throwError(() => new Error('error')));
    
    routeQueryParamMap.next({ get: (key: string) => key === 'name' ? 'Test Agency' : null });
    
    fixture.detectChanges();
    
    expect(component.packages().length).toBe(0);
    expect(component.isLoading()).toBeFalsy();
  });

  it('should return correct primary image', () => {
    expect(component.getPrimaryImage({} as any)).toContain('aida-public');
    expect(component.getPrimaryImage({ primaryImageUrl: 'http://img.jpg' } as any)).toBe('http://img.jpg');
    expect(component.getPrimaryImage({ primaryImageUrl: '/img.jpg' } as any)).toBe(`${environment.baseUrl}/img.jpg`);
  });

  it('should return correct profile image', () => {
    expect(component.getProfileImage()).toBeNull();
    expect(component.getProfileImage('http://profile.jpg')).toBe('http://profile.jpg');
    expect(component.getProfileImage('pic.jpg')).toBe(`${environment.apiUrl}/Users/profile/picture/pic.jpg`);
  });

  it('should format website url correctly', () => {
    expect(component.formatWebsiteUrl('')).toBe('');
    expect(component.formatWebsiteUrl('example.com')).toBe('https://example.com');
    expect(component.formatWebsiteUrl('http://example.com')).toBe('http://example.com');
  });

  it('should return starting price', () => {
    expect(component.getStartingPrice({ startingPrice: 100 } as any)).toBe(100);
    expect(component.getStartingPrice({} as any)).toBe(0);
  });

  it('should navigate to view package details', () => {
    fixture.detectChanges();
    component.viewPackageDetails('pkg-1');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/package', 'pkg-1']);
  });
});
