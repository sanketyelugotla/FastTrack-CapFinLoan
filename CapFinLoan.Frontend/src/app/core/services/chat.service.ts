import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ChatRequest, ChatResponse } from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/chat`;

  sendMessage(message: string, conversationHistory: { role: string; content: string }[]) {
    const request: ChatRequest = { message, conversationHistory };
    return this.http.post<ChatResponse>(`${this.apiUrl}/message`, request);
  }
}
