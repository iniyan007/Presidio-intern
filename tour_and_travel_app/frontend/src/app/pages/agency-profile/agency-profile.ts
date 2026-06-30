import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AgencyService } from '../../services/agency.service';
import { PublicPackagerResponse, PackagerReviewResponse } from '../../models/packager.model';
import { PackageService } from '../../services/package.service';
import { TravelPackage } from '../../models/package.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-agency-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './agency-profile.html'
})
export class AgencyProfileComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private packagerService = inject(AgencyService);
  private packageService = inject(PackageService);

  packager = signal<PublicPackagerResponse | null>(null);
  reviews = signal<PackagerReviewResponse[]>([]);
  packages = signal<TravelPackage[]>([]);
  
  isLoading = signal<boolean>(true);
  errorMessage = signal<string>('');

  ngOnInit() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const packageId = params.get('packageId');
      
      this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(queryParams => {
        const name = queryParams.get('name');
        
        if (name) {
          this.loadProfile(name);
        } else if (packageId && packageId !== 'view') {
          this.loadPackagerProfileByPackageId(packageId);
        } else {
          this.errorMessage.set('Invalid profile link.');
          this.isLoading.set(false);
        }
      });
    });
  }

  loadPackagerProfileByPackageId(packageId: string) {
    this.isLoading.set(true);
    
    // 1. First fetch the package to get the packager name
    this.packageService.getPackageById(packageId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (pkgDetails) => {
        this.loadProfile(pkgDetails.packagerName);
      },
      error: (err: any) => {
        console.error('Failed to load package details', err);
        this.errorMessage.set('Could not load package to find packager.');
        this.isLoading.set(false);
      }
    });
  }

  loadProfile(packagerName: string) {
    this.isLoading.set(true);
    // 2. Fetch the packager details using the existing search API
    this.packagerService.searchPublicPackagers(packagerName).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        const items = res.items ? res.items : (res.data ? res.data : res);
        if (items && items.length > 0) {
          const packager = items[0];
          this.packager.set(packager);
          
          // 3. Fetch packages for this packager
          this.packageService.getPackages({ PackagerName: packager.companyName }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
            next: (pkgRes: any) => {
              const pkgs = pkgRes.items ? pkgRes.items : (pkgRes.data ? pkgRes.data : pkgRes);
              this.packages.set(pkgs || []);
            },
            error: (err: any) => console.error('Failed to load packager packages', err)
          });

          // 4. Fetch reviews using the ID
          this.packagerService.getPackagerReviews(packager.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
            next: (reviewsRes: PackagerReviewResponse[]) => {
              this.reviews.set(reviewsRes);
              this.isLoading.set(false);
            },
            error: (err: any) => {
              console.error('Failed to load packager reviews', err);
              this.isLoading.set(false);
            }
          });
        } else {
          this.errorMessage.set('Could not find packager details.');
          this.isLoading.set(false);
        }
      },
      error: (err: any) => {
        console.error('Failed to search packager', err);
        this.errorMessage.set('Could not search packager details.');
        this.isLoading.set(false);
      }
    });
  }

  getPrimaryImage(pkg: TravelPackage): string {
    if (!pkg.primaryImageUrl) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE';
    }
    return pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `${environment.baseUrl}${pkg.primaryImageUrl}`;
  }

  getProfileImage(url?: string): string | null {
    if (!url) return null;
    return url.startsWith('http') ? url : `${environment.apiUrl}/Users/profile/picture/${url}`;
  }

  formatWebsiteUrl(url: string): string {
    if (!url) return '';
    return url.startsWith('http://') || url.startsWith('https://') ? url : `https://${url}`;
  }

  getStartingPrice(pkg: TravelPackage): number {
    return pkg.startingPrice || 0;
  }

  viewPackageDetails(id: string) {
    this.router.navigate(['/package', id]);
  }
}
