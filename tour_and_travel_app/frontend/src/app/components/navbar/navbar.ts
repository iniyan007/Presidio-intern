import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';

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

  getProfileImageUrl(fileName: string): string {
    return `http://localhost:5082/api/Users/profile/picture/${fileName}`;
  }

  logout() {
    this.authService.logout();
    this.toastService.show('Logged out successfully', 'info');
    this.router.navigate(['/']);
  }
}
