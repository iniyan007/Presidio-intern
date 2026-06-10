import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { userInfoSubject, UserInfo, restoreAuthState } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit, OnDestroy {
  userInfo = signal<UserInfo | null>(null);
  private userInfoSubscription: any;

  ngOnInit() {
    restoreAuthState();

    const stored = sessionStorage.getItem('userInfo');
    if (stored) {
      try {
        this.userInfo.set(JSON.parse(stored));
      } catch {
        this.userInfo.set(null);
      }
    }

    this.userInfoSubscription = userInfoSubject.subscribe((info) => {
      if (info) {
        this.userInfo.set(info);
      }
    });
  }

  ngOnDestroy() {
    this.userInfoSubscription?.unsubscribe();
  }
}

