import { Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PackageService } from '../../services/package.service';
import { UserService } from '../../services/user.service';
import { environment } from '../../../environments/environment';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-manage-packages',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './manage-packages.html'
})
export class ManagePackagesComponent {
  myPackages = signal<any[]>([]);
  isLoading = signal(true);

  private packageService = inject(PackageService);
  private userService = inject(UserService);
  private router = inject(Router);

  constructor() {
    effect(() => {
      const profile = this.userService.userProfile();
      if (profile && profile.fullName) {
        this.loadPackages();
      }
    });
  }

  private loadPackages() {
    this.isLoading.set(true);
    this.packageService.getMyPackages().subscribe({
      next: (res) => {
        const packages = res;
        const packageList = packages.map((pkg: any) => {
          let imgUrl = 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE'; // default
          if (pkg.primaryImageUrl) {
            imgUrl = pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `${environment.baseUrl}${pkg.primaryImageUrl}`;
          }

          return {
            id: pkg.id,
            title: pkg.title,
            durationDays: pkg.durationDays,
            status: pkg.status || 'Active',
            slotsLeft: pkg.pendingSeats || 0,
            price: pkg.startingPrice,
            imageUrl: imgUrl
          };
        });

        this.myPackages.set(packageList);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onEditPackage(packageId: string) {
    this.router.navigate(['/packager/edit-package', packageId]);
  }
}
