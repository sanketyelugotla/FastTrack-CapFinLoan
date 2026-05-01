import {
  Component,
  inject,
  signal,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ChatbotService } from '../../../core/services/chatbot.service';
import { ChatMessage, ChatAction } from '../../../core/models/chatbot.models';

let msgCounter = 0;
const uid = () => `msg-${++msgCounter}`;

@Component({
  selector: 'app-chatbot-widget',
  imports: [DatePipe],
  templateUrl: './chatbot-widget.component.html',
  styleUrl: './chatbot-widget.component.css',
})
export class ChatbotWidgetComponent implements OnInit, AfterViewChecked {
  private chatService = inject(ChatbotService);
  private router = inject(Router);

  @ViewChild('messageList') messageList!: ElementRef<HTMLDivElement>;

  // ── State ────────────────────────────────────────────────────
  isOpen = signal(false);
  isTyping = signal(false);
  inputText = signal('');
  messages = signal<ChatMessage[]>([]);
  private shouldScroll = false;

  ngOnInit(): void {
    this.addBotMessage(
      "Hi there! 👋 I'm **CapBot**, your loan assistant. I can help you apply for a loan, check your application status, or answer questions.\n\nWhat would you like to do?",
      ['Apply for a loan', 'Check my application', 'Loan eligibility']
    );
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  // ── Toggle panel ─────────────────────────────────────────────
  toggle(): void {
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      this.shouldScroll = true;
    }
  }

  close(): void {
    this.isOpen.set(false);
  }

  // ── Sending messages ─────────────────────────────────────────
  sendMessage(text?: string): void {
    const msg = (text ?? this.inputText()).trim();
    if (!msg) return;

    this.inputText.set('');
    this.addUserMessage(msg);
    this.isTyping.set(true);

    this.chatService.sendMessage(msg).subscribe({
      next: (res) => {
        this.chatService.persistSessionId(res.session_id);
        this.isTyping.set(false);
        this.addBotMessage(res.reply, res.quick_replies, res.action, res.progress ? { step: res.progress.step, total: res.progress.total, label: res.progress.label } : undefined);
        if (res.action) {
          this.handleAction(res.action);
        }
      },
      error: () => {
        this.isTyping.set(false);
        this.addBotMessage(
          "I'm having trouble connecting right now. Please try again in a moment.",
          ['Try again', 'Talk to an agent']
        );
      },
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  sendQuickReply(reply: string): void {
    this.sendMessage(reply);
  }

  // ── Action handling ───────────────────────────────────────────
  private handleAction(action: ChatAction): void {
    switch (action.type) {
      case 'navigate':
        if (action.payload) {
          setTimeout(() => {
            this.router.navigate([action.payload]);
            this.close();
          }, 1200);
        }
        break;

      case 'show_status':
        if (action.payload) {
          setTimeout(() => {
            this.router.navigate([action.payload]);
          }, 1200);
        }
        break;

      case 'upload_document':
        setTimeout(() => {
          this.router.navigate([action.payload || '/applicant/documents']);
          this.close();
        }, 1200);
        break;

      case 'escalate':
        // Already shown in the message bubble
        break;

      case 'pre_fill_form':
        // These are shown as action cards in the bubble
        break;
    }
  }

  handleActionCard(action: ChatAction | null | undefined): void {
    if (!action) return;
    this.handleAction(action);
  }

  // ── Clear session ─────────────────────────────────────────────
  clearChat(): void {
    this.chatService.clearSession().subscribe();
    this.messages.set([]);
    this.ngOnInit();
  }

  // ── Helpers ──────────────────────────────────────────────────
  private addUserMessage(text: string): void {
    this.messages.update(msgs => [
      ...msgs,
      { id: uid(), role: 'user', text, timestamp: new Date() },
    ]);
    this.shouldScroll = true;
  }

  private addBotMessage(
    text: string,
    quickReplies: string[] = [],
    action?: ChatAction | null,
    progress?: { step: number; total: number; label: string }
  ): void {
    this.messages.update(msgs => [
      ...msgs,
      {
        id: uid(),
        role: 'bot',
        text,
        timestamp: new Date(),
        quickReplies,
        action,
        progress,
      },
    ]);
    this.shouldScroll = true;
  }

  private scrollToBottom(): void {
    try {
      const el = this.messageList?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch { }
  }

  /** Render **bold** markdown in bot replies */
  formatText(text: string): string {
    return text
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\n/g, '<br>');
  }

  getProgressPercent(step: number, total: number): number {
    return Math.round((step / total) * 100);
  }
}
