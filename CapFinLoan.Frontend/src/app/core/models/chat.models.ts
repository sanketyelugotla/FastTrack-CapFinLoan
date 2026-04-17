export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  actionsTaken?: ChatActionResult[];
  suggestions?: string[];
}

export interface ChatActionResult {
  action: string;
  status: 'success' | 'failed';
  details: string;
}

export interface ChatResponse {
  reply: string;
  actionsTaken: ChatActionResult[];
  suggestions: string[];
}

export interface ChatRequest {
  message: string;
  conversationHistory: { role: string; content: string }[];
}
