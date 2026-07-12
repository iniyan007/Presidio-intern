import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { MessageResponse, MessageThreadResponse, SendMessageRequest, CreateThreadRequest } from '../models/chat.model';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Messages`;
  private hubUrl = `${environment.baseUrl}/hubs/chat`;
  private hubConnection: signalR.HubConnection | null = null;

  // Real-time signals
  public threads = signal<MessageThreadResponse[]>([]);
  currentThreadMessages = signal<MessageResponse[]>([]);
  newMessageReceived = signal<MessageResponse | null>(null);
  activeThreadId = signal<string | null>(null);

  constructor() { }

  // API Calls
  public getOrInitializeThread(request: CreateThreadRequest): Observable<MessageThreadResponse> {
    return this.http.post<MessageThreadResponse>(`${this.apiUrl}/threads/init`, request);
  }

  public getThreads(): Observable<MessageThreadResponse[]> {
    return this.http.get<MessageThreadResponse[]>(`${this.apiUrl}/threads`);
  }

  public getThreadMessages(threadId: string): Observable<MessageResponse[]> {
    return this.http.get<MessageResponse[]>(`${this.apiUrl}/threads/${threadId}/messages`);
  }

  public sendMessage(request: SendMessageRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/send`, request);
  }

  public markAsRead(threadId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/threads/${threadId}/read`, {});
  }

  // SignalR Hub Management
  public startConnection(token: string) {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, { accessTokenFactory: () => localStorage.getItem('jwt_token') || '' })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Chat SignalR Connection started'))
      .catch(err => console.log('Error while starting Chat SignalR connection: ' + err));

    this.hubConnection.on('ReceiveMessage', (message: MessageResponse) => {
      this.newMessageReceived.set(message);
      
      // If we are currently viewing this thread, append it to messages
      if (this.activeThreadId() === message.threadId) {
        const currentMessages = this.currentThreadMessages();
        if (!currentMessages.find(m => m.id === message.id)) {
          this.currentThreadMessages.set([...currentMessages, message]);
          // Also auto-mark as read if active
          this.markAsRead(message.threadId).subscribe();
        }
      }

      // Update thread list last message details
      const currentThreads = this.threads();
      const updatedThreads = currentThreads.map(t => {
        if (t.id === message.threadId) {
          return { 
            ...t, 
            lastMessageAt: message.sentAt,
            // If not active thread, increment unread count
            unreadCount: (this.activeThreadId() === message.threadId) ? t.unreadCount : t.unreadCount + 1
          };
        }
        return t;
      });
      
      // Move updated thread to top
      const threadIndex = updatedThreads.findIndex(t => t.id === message.threadId);
      if (threadIndex > 0) {
        const t = updatedThreads.splice(threadIndex, 1)[0];
        updatedThreads.unshift(t);
      }
      
      this.threads.set(updatedThreads);
    });
  }

  public stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  public joinThread(threadId: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinThread', threadId)
        .catch(err => console.error(err));
    }
  }

  public leaveThread(threadId: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('LeaveThread', threadId)
        .catch(err => console.error(err));
    }
  }
}
