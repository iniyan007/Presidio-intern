import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { WishlistResponse } from '../models/package.model';
import { environment } from '../../environments/environment';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private http = inject(HttpClient);
  private toastService = inject(ToastService);
  private apiUrl = `${environment.apiUrl}/Wishlists`;

  wishlistedPackageIds = signal<Set<string>>(new Set());

  loadWishlists(): void {
    this.http.get<{ success: boolean; data: WishlistResponse[] }>(this.apiUrl).subscribe({
      next: (res) => {
        const ids = res.data.map(w => w.packageId);
        this.wishlistedPackageIds.set(new Set(ids));
      },
      error: (err) => {
        console.error('Failed to load wishlists', err);
      }
    });
  }

  clearWishlists(): void {
    this.wishlistedPackageIds.set(new Set());
  }

  getWishlists(): Observable<{ success: boolean; data: WishlistResponse[] }> {
    return this.http.get<{ success: boolean; data: WishlistResponse[] }>(this.apiUrl);
  }

  toggleWishlist(packageId: string, packageName?: string): void {
    this.http.post<{ success: boolean; message: string; added: boolean }>(`${this.apiUrl}/toggle/${packageId}`, {}).subscribe({
      next: (res) => {
        this.wishlistedPackageIds.update(currentSet => {
          const newSet = new Set(currentSet);
          if (res.added) {
            newSet.add(packageId);
          } else {
            newSet.delete(packageId);
          }
          return newSet;
        });
        
        const toastMsg = packageName 
          ? (res.added ? `"${packageName}" added to wishlist` : `"${packageName}" removed from wishlist`)
          : res.message;
          
        this.toastService.show(toastMsg, 'success');
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Failed to update wishlist.', 'error');
      }
    });
  }
}
