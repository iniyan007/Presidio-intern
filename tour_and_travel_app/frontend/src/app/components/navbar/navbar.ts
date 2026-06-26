import { Component, inject, OnInit, signal, effect, HostListener, ElementRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { WishlistService } from '../../services/wishlist.service';
import { NotificationService } from '../../services/notification.service';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent {
  authService = inject(AuthService);
  private router = inject(Router);
  userService = inject(UserService);
  private toastService = inject(ToastService);
  wishlistService = inject(WishlistService);
  notificationService = inject(NotificationService);
  chatService = inject(ChatService);
  private eRef = inject(ElementRef);

  isNotificationOpen = signal(false);

  constructor() {
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.notificationService.loadNotifications();
        this.notificationService.startConnection();
        
        const token = localStorage.getItem('jwt_token');
        if (token) {
          this.chatService.startConnection(token);
          this.chatService.getThreads().subscribe({
            next: (threads) => this.chatService.threads.set(threads)
          });
        }
      } else {
        this.notificationService.stopConnection();
        this.chatService.stopConnection();
      }
    });
  }

  get unreadChatCount(): number {
    return this.chatService.threads().reduce((acc, t) => acc + t.unreadCount, 0);
  }

  toggleNotification() {
    this.isNotificationOpen.set(!this.isNotificationOpen());
  }

  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (this.isNotificationOpen()) {
      const clickedInside = this.eRef.nativeElement.querySelector('.notification-container')?.contains(event.target);
      if (!clickedInside) {
        this.isNotificationOpen.set(false);
      }
    }
  }

  markAsRead(id: string) {
    this.notificationService.markAsRead(id);
    this.isNotificationOpen.set(false);
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead();
  }

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
