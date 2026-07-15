import { Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService, ChatMessage } from '../../services/ai.service';
import { AuthService } from '../../services/auth.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-chat.html',
  styleUrl: './ai-chat.css'
})
export class AiChatComponent {
  isOpen = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  userInput = signal<string>('');
  
  messages = signal<ChatMessage[]>([
    { role: 'ai', text: 'Hi there! I am your TourMate Travel Concierge. How can I help you plan your next adventure?' }
  ]);

  @ViewChild('chatScroll') private chatScroll!: ElementRef;

  private aiService = inject(AiService);
  authService = inject(AuthService);

  toggleChat() {
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  sendMessage() {
    const text = this.userInput().trim();
    if (!text) return;

    // Add user message
    this.messages.update(msgs => [...msgs, { role: 'user', text }]);
    this.userInput.set('');
    this.isLoading.set(true);
    this.scrollToBottom();

    // Call API
    this.aiService.sendMessage(this.messages()).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.messages.update(msgs => [...msgs, { role: 'ai', text: res.data.reply }]);
        this.scrollToBottom();
      },
      error: () => {
        this.isLoading.set(false);
        this.messages.update(msgs => [...msgs, { role: 'ai', text: 'Sorry, I am having trouble connecting to the network right now.' }]);
        this.scrollToBottom();
      }
    });
  }

  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom() {
    setTimeout(() => {
      if (this.chatScroll) {
        this.chatScroll.nativeElement.scrollTop = this.chatScroll.nativeElement.scrollHeight;
      }
    }, 50);
  }
}
