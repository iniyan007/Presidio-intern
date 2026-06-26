import { Component, effect, inject, signal, ViewChild, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { UserService } from '../../services/user.service';
import { MessageThreadResponse, MessageResponse } from '../../models/chat.model';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class ChatComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);
  public userService = inject(UserService);
  private route = inject(ActivatedRoute);

  threads = this.chatService.threads;
  messages = this.chatService.currentThreadMessages;
  activeThreadId = signal<string | null>(null);
  
  newMessage = signal<string>('');
  isPackager = signal<boolean>(false);

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  private authService = inject(AuthService);

  constructor() {
    effect(() => {
      const profile = this.userService.userProfile();
      if (profile) {
        this.isPackager.set(this.authService.getUserRole() === 'Packager');
        const token = localStorage.getItem('jwt_token');
        if (token) {
          this.chatService.startConnection(token);
        }
      }
    });

    effect(() => {
      // scroll to bottom on new messages
      if (this.messages().length > 0) {
        setTimeout(() => this.scrollToBottom(), 100);
      }
    });
  }

  ngOnInit() {
    this.loadThreads();
    
    // Check if opened with a specific thread ID
    this.route.queryParams.subscribe(params => {
      if (params['threadId']) {
        // give it a short delay to load threads first
        setTimeout(() => this.selectThread(params['threadId']), 500);
      }
    });
  }

  ngOnDestroy() {
    if (this.activeThreadId()) {
      this.chatService.leaveThread(this.activeThreadId()!);
      this.chatService.activeThreadId.set(null);
    }
    this.chatService.stopConnection();
  }

  loadThreads() {
    this.chatService.getThreads().subscribe({
      next: (threads) => this.chatService.threads.set(threads)
    });
  }

  selectThread(threadId: string) {
    if (this.activeThreadId()) {
      this.chatService.leaveThread(this.activeThreadId()!);
    }
    this.activeThreadId.set(threadId);
    this.chatService.activeThreadId.set(threadId);
    this.chatService.joinThread(threadId);

    // Fetch messages
    this.chatService.getThreadMessages(threadId).subscribe({
      next: (msgs) => {
        this.chatService.currentThreadMessages.set(msgs);
        this.chatService.markAsRead(threadId).subscribe(() => {
           // Update local unread count
           const currentThreads = this.threads();
           const updated = currentThreads.map(t => {
             if (t.id === threadId) {
               return { ...t, unreadCount: 0 };
             }
             return t;
           });
           this.chatService.threads.set(updated);
        });
      }
    });
  }

  sendMessage() {
    if (!this.newMessage().trim() || !this.activeThreadId()) return;

    const request = {
      threadId: this.activeThreadId()!,
      senderRole: this.isPackager() ? 1 : 0,
      body: this.newMessage().trim()
    };

    this.chatService.sendMessage(request).subscribe({
      next: (msg) => {
        this.newMessage.set('');
        // Append locally for immediate UI update
        const currentMessages = this.chatService.currentThreadMessages();
        if (!currentMessages.find(m => m.id === msg.id)) {
          this.chatService.currentThreadMessages.set([...currentMessages, msg]);
          setTimeout(() => this.scrollToBottom(), 100);
        }
      }
    });
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }

  getActiveThread(): MessageThreadResponse | undefined {
    return this.threads().find(t => t.id === this.activeThreadId());
  }

  getThreadTitle(thread: MessageThreadResponse): string {
    return this.isPackager() ? thread.userName : thread.packagerName;
  }
}
