import { Component, inject, signal, computed, ElementRef, ViewChild, AfterViewChecked, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChatMessage } from '../../../core/models/chat.models';

@Component({
  selector: 'app-chat-widget',
  imports: [FormsModule],
  templateUrl: './chat-widget.component.html',
  styleUrl: './chat-widget.component.css'
})
export class ChatWidgetComponent implements AfterViewChecked {
  private chatService = inject(ChatService);
  private authService = inject(AuthService);

  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  @ViewChild('messageInput') private messageInput!: ElementRef;

  isOpen = signal(false);
  messages = signal<ChatMessage[]>([]);
  inputMessage = '';
  sending = signal(false);
  error = signal('');
  private shouldScroll = false;

  isAdmin = computed(() => this.authService.isAdmin());
  userName = computed(() => this.authService.currentUser()?.name ?? 'User');

  welcomeMessage = computed(() => {
    if (this.isAdmin()) {
      return `Hello ${this.userName()}! I'm your Admin Assistant. I can help you manage the application queue, review applications, verify documents, and make approval decisions. What would you like to do?`;
    }
    return `Hello ${this.userName()}! I'm CapFin, your loan application assistant. I can help you understand the application form, check your application status, calculate EMI, and answer questions about the loan process. How can I help?`;
  });

  suggestions = computed(() => {
    if (this.messages().length > 0) {
      const last = this.messages()[this.messages().length - 1];
      if (last.suggestions?.length) return last.suggestions;
    }
    if (this.isAdmin()) {
      return ['Show dashboard', 'Show submitted applications', 'Which applications need attention?', 'Show pending documents'];
    }
    return ['What documents do I need?', 'Show my applications', 'Calculate my EMI', 'Help me fill the form'];
  });

  ngAfterViewChecked() {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  toggleChat() {
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      setTimeout(() => this.messageInput?.nativeElement?.focus(), 200);
    }
  }

  sendMessage(text?: string) {
    const message = (text ?? this.inputMessage).trim();
    if (!message || this.sending()) return;

    this.inputMessage = '';
    this.error.set('');

    // Add user message
    const userMsg: ChatMessage = {
      role: 'user',
      content: message,
      timestamp: new Date().toISOString()
    };
    this.messages.update(msgs => [...msgs, userMsg]);
    this.shouldScroll = true;
    this.sending.set(true);

    // Build conversation history (exclude the last user message we just added)
    const history = this.messages()
      .slice(0, -1)
      .map(m => ({ role: m.role, content: m.content }));

    this.chatService.sendMessage(message, history).subscribe({
      next: (response) => {
        const assistantMsg: ChatMessage = {
          role: 'assistant',
          content: response.reply,
          timestamp: new Date().toISOString(),
          actionsTaken: response.actionsTaken?.length ? response.actionsTaken : undefined,
          suggestions: response.suggestions?.length ? response.suggestions : undefined
        };
        this.messages.update(msgs => [...msgs, assistantMsg]);
        this.shouldScroll = true;
        this.sending.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to get a response. Please try again.');
        this.sending.set(false);
        this.shouldScroll = true;
      }
    });
  }

  useSuggestion(suggestion: string) {
    this.sendMessage(suggestion);
  }

  clearChat() {
    this.messages.set([]);
    this.error.set('');
  }

  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom() {
    try {
      const el = this.messagesContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch { }
  }

  formatContent(content: string): string {
    // Basic markdown-like formatting for chat messages
    return content
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.+?)\*/g, '<em>$1</em>')
      .replace(/`(.+?)`/g, '<code>$1</code>')
      .replace(/\n/g, '<br>');
  }
}
