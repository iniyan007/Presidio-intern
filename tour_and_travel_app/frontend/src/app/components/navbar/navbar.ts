import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  userService = inject(UserService);

  isLoggedIn = signal<boolean>(false);

  ngOnInit() {
    this.isLoggedIn.set(!!this.authService.getToken());
  }

  getProfileImageUrl(fileName: string): string {
    return `http://localhost:5082/api/Users/profile/picture/${fileName}`;
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn.set(false);
    this.router.navigate(['/']);
  }
}
