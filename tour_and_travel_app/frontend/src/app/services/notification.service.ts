import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { AppNotification } from '../models/notification.model';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private hubConnection: signalR.HubConnection | null = null;
  
  notifications = signal<AppNotification[]>([]);
  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

  private apiUrl = `${environment.apiUrl}/Notifications`;
  private hubUrl = `${environment.baseUrl}/hubs/notifications`;

  constructor() {
    if (this.authService.isAuthenticated()) {
      this.loadNotifications();
      this.startConnection();
    }
  }

  loadNotifications() {
    this.http.get<{ success: boolean; data: AppNotification[] }>(this.apiUrl).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.notifications.set(res.data);
        }
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  startConnection() {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Notification Hub Connected'))
      .catch(err => console.error('Error connecting to Notification Hub', err));

    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      this.notifications.update(current => [notification, ...current]);
      this.toastService.show(notification.title, 'success');
    });
  }

  stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
    this.notifications.set([]);
  }

  markAsRead(id: string) {
    this.notifications.update(current => 
      current.map(n => n.id === id ? { ...n, isRead: true } : n)
    );
    this.http.put(`${this.apiUrl}/${id}/read`, {}).subscribe({
      error: (err) => {
        console.error('Failed to mark as read', err);
      }
    });
  }

  markAllAsRead() {
    this.notifications.update(current => 
      current.map(n => ({ ...n, isRead: true }))
    );
    this.http.put(`${this.apiUrl}/read-all`, {}).subscribe({
      error: (err) => {
        console.error('Failed to mark all as read', err);
      }
    });
  }
}
