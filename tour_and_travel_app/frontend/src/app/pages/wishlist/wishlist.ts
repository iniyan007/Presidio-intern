import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { WishlistService } from '../../services/wishlist.service';
import { WishlistResponse } from '../../models/package.model';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './wishlist.html'
})
export class WishlistComponent implements OnInit {
  wishlistService = inject(WishlistService);
  private router = inject(Router);

  wishlists = signal<WishlistResponse[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string>('');

  ngOnInit() {
    this.loadWishlists();
  }

  loadWishlists() {
    this.isLoading.set(true);
    this.wishlistService.getWishlists().subscribe({
      next: (res) => {
        this.wishlists.set(res.data || []);
        // Also update the service's Set for consistency
        const ids = res.data.map(w => w.packageId);
        this.wishlistService.wishlistedPackageIds.set(new Set(ids));
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Failed to load wishlist. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  toggleWishlist(packageId: string, packageName?: string) {
    this.wishlistService.toggleWishlist(packageId, packageName);
    // Optimistically remove from local view
    this.wishlists.update(current => current.filter(w => w.packageId !== packageId));
  }

  viewDetails(packageId: string) {
    this.router.navigate(['/package', packageId]);
  }

  getPrimaryImage(pkg: any): string {
    if (!pkg.primaryImageUrl) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCoMzMsdBS9393A5TXkJBkEbxwXe0a18-RDlN-FdC8d3zQd3pQ04WfHxEfLXcQnuERcC2V82jfEdlQiTtSMdhhAuWKFia-1L0C-mUbwtIxZAhKPMEdXj_Z0atOnnXmUoZWYPwSFF33dxFjviNUOqQoBRCIYQyyvK36Az4cVRWQcXWakicjyqlrZ9fHv4fV4WaBmMHKV29xM4GyOwzxpZsA0g0fuiRC5Z_6CYP_VbA-dMBvI4aqOLaVRDDB4lkqbctFMmUNYNTQ1AE';
    }
    return pkg.primaryImageUrl.startsWith('http') ? pkg.primaryImageUrl : `${environment.baseUrl}${pkg.primaryImageUrl}`;
  }

  getStartingPrice(pkg: any): number {
    return pkg.startingPrice || 0;
  }
}
