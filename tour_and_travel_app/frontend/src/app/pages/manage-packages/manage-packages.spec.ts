import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ManagePackagesComponent } from './manage-packages';
import { PackageService } from '../../services/package.service';
import { UserService } from '../../services/user.service';
import { Router } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';

describe('ManagePackagesComponent', () => {
  let component: ManagePackagesComponent;
  let fixture: ComponentFixture<ManagePackagesComponent>;

  let packageServiceSpy: any;
  let userServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    packageServiceSpy = {
      getMyPackages: vi.fn().mockReturnValue(of([]))
    };
    userServiceSpy = {
      userProfile: signal(null)
    };

    await TestBed.configureTestingModule({
      imports: [ManagePackagesComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: PackageService, useValue: packageServiceSpy },
        { provide: UserService, useValue: userServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManagePackagesComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load packages when user profile is set', () => {
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(packageServiceSpy.getMyPackages).toHaveBeenCalled();
  });

  it('should format loaded packages correctly', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(of([
      { id: 'pkg1', title: 'Package 1', durationDays: 5, pendingSeats: 10, startingPrice: 100, packageType: 'Adventure' }, // no image
      { id: 'pkg2', title: 'Package 2', durationDays: 3, pendingSeats: 5, startingPrice: 200, packageType: 'Relax', primaryImageUrl: 'http://img.jpg' }, // absolute image
      { id: 'pkg3', title: 'Package 3', durationDays: 7, startingPrice: 300, packageType: 'Cultural', primaryImageUrl: '/img.jpg' } // relative image
    ]));
    
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(component.myPackages().length).toBe(3);
    expect(component.myPackages()[0].imageUrl).toContain('aida-public');
    expect(component.myPackages()[0].slotsLeft).toBe(10);
    expect(component.myPackages()[1].imageUrl).toBe('http://img.jpg');
    expect(component.myPackages()[2].imageUrl).toBe(`${environment.baseUrl}/img.jpg`);
    expect(component.myPackages()[2].slotsLeft).toBe(0);
    expect(component.isLoading()).toBeFalsy();
  });

  it('should handle package loading error gracefully', () => {
    packageServiceSpy.getMyPackages.mockReturnValue(throwError(() => new Error('Error')));
    
    userServiceSpy.userProfile.set({ id: 'user-1', fullName: 'John Doe' });
    fixture.detectChanges();
    
    expect(component.myPackages().length).toBe(0);
    expect(component.isLoading()).toBeFalsy();
  });

  it('should navigate to edit package', () => {
    fixture.detectChanges();
    component.onEditPackage('pkg-1');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/agency/edit-package', 'pkg-1']);
  });
});
