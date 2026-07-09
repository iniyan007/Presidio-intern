import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatMessage {
  role: 'user' | 'ai';
  text: string;
}

export interface ChatRequest {
  messages: ChatMessage[];
}

export interface ChatResponse {
  success: boolean;
  data: {
    reply: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Ai/chat`;

  sendMessage(messages: ChatMessage[]): Observable<ChatResponse> {
    const request: ChatRequest = { messages };
    return this.http.post<ChatResponse>(this.apiUrl, request);
  }
}
