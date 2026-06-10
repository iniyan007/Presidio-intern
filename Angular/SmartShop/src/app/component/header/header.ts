import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { usernameSubject, userInfoSubject, isLoggedIn, logout } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit, OnDestroy {
  username = signal<string | undefined>(undefined);
  firstName = signal<string | undefined>(undefined);
  lastName = signal<string | undefined>(undefined);
  loggedIn = signal<boolean>(false);
  private usernameSubcription: any;
  private userInfoSubscription: any;

  ngOnInit(): void {
    this.loggedIn.set(isLoggedIn());
    
    this.userInfoSubscription = userInfoSubject.subscribe((info) => {
      if (info) {
        this.firstName.set(info.firstName);
        this.lastName.set(info.lastName);
        this.loggedIn.set(true);
      }
    });

    this.usernameSubcription = usernameSubject.subscribe((name) => {
      this.username.set(name);
      this.loggedIn.set(Boolean(name) || isLoggedIn());
    });
  }

  ngOnDestroy(): void {
    this.usernameSubcription?.unsubscribe();
    this.userInfoSubscription?.unsubscribe();
  }

  logout() {
    logout();
    this.username.set(undefined);
    this.firstName.set(undefined);
    this.lastName.set(undefined);
    this.loggedIn.set(false);
  }
}
