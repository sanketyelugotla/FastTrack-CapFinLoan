/**
 * TypeScript models matching the Python ChatbotService API contracts.
 */

export interface ChatRequest {
  session_id: string | null;
  message: string;
}

export interface ChatAction {
  type: 'navigate' | 'upload_document' | 'pre_fill_form' | 'show_status' | 'escalate';
  payload: string | null;
}

export interface ChatProgress {
  step: number;
  total: number;
  label: string;
}

export interface ChatResponse {
  session_id: string;
  reply: string;
  quick_replies: string[];
  action: ChatAction | null;
  progress: ChatProgress | null;
}

/** Internal UI model for displaying messages in the chat window */
export interface ChatMessage {
  id: string;
  role: 'user' | 'bot';
  text: string;
  timestamp: Date;
  quickReplies?: string[];
  action?: ChatAction | null;
  progress?: ChatProgress | null;
}
