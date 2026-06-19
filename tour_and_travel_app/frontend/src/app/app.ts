import { Component, signal, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar';
import { ToastComponent } from './components/toast/toast';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('frontend');
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    if (this.authService.isAuthenticated() && !this.authService.isEmailVerified()) {
      if (this.router.url !== '/verify-email') {
        this.router.navigate(['/verify-email']);
      }
    }
  }
}
