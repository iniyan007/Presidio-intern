import { Component, signal, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar';
import { ToastComponent } from './components/toast/toast';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs';

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
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      if (this.authService.isAuthenticated() && !this.authService.isEmailVerified()) {
        if (event.urlAfterRedirects !== '/verify-email') {
          this.router.navigate(['/verify-email']);
        }
      }
    });

    if (this.authService.isAuthenticated() && !this.authService.isEmailVerified()) {
      if (this.router.url !== '/verify-email') {
        this.router.navigate(['/verify-email']);
      }
    }
  }
}
