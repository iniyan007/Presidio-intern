import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { logout, userInfoSubject } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  firstName = signal<string | undefined>(undefined);
  lastName = signal<string | undefined>(undefined);
  private userInfoSubscription: any;

  constructor(private router: Router) {}

  ngOnInit() {
    this.userInfoSubscription = userInfoSubject.subscribe((info) => {
      this.firstName.set(info?.firstName);
      this.lastName.set(info?.lastName);
    });
  }

  ngOnDestroy() {
    this.userInfoSubscription?.unsubscribe();
  }
  
  handleLogout() {
      logout();
      this.router.navigate(['login']);
  }
}
