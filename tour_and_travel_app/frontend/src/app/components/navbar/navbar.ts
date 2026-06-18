import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { WishlistService } from '../../services/wishlist.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent {
  authService = inject(AuthService);
  private router = inject(Router);
  userService = inject(UserService);
  private toastService = inject(ToastService);
  wishlistService = inject(WishlistService);

  getProfileImageUrl(fileName: string): string {
    return `${environment.apiUrl}/Users/profile/picture/${fileName}`;
  }

  logout() {
    this.authService.logout();
    this.wishlistService.clearWishlists();
    this.toastService.show('Logged out successfully', 'info');
    this.router.navigate(['/']);
  }
}
