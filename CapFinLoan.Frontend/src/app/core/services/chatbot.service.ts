import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatRequest, ChatResponse } from '../models/chatbot.models';

const SESSION_KEY = 'capfinloan_chat_session';

@Injectable({ providedIn: 'root' })
export class ChatbotService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/chat`;

  /** Get or create a session ID persisted in sessionStorage */
  getSessionId(): string | null {
    return sessionStorage.getItem(SESSION_KEY);
  }

  private saveSessionId(id: string): void {
    sessionStorage.setItem(SESSION_KEY, id);
  }

  /** Send a user message and receive a bot response */
  sendMessage(message: string): Observable<ChatResponse> {
    const body: ChatRequest = {
      session_id: this.getSessionId(),
      message,
    };
    return this.http.post<ChatResponse>(`${this.baseUrl}`, body);
  }

  /** Store the session ID returned from the first bot response */
  persistSessionId(id: string): void {
    this.saveSessionId(id);
  }

  /** Clear the session */
  clearSession(): Observable<unknown> {
    sessionStorage.removeItem(SESSION_KEY);
    return this.http.delete(`${this.baseUrl}/session`);
  }

  /** Health ping (optional, for debug) */
  healthCheck(): Observable<unknown> {
    return this.http.get(`${this.baseUrl}/health`);
  }
}
