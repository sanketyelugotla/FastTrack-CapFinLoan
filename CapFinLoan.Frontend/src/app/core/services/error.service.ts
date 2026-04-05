import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: number;
  text: string;
  type: 'error' | 'success' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ErrorService {
  private _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();
  private nextId = 0;

  showError(message: string, duration = 6000) {
    this.push(message, 'error', duration);
  }

  showSuccess(message: string, duration = 4000) {
    this.push(message, 'success', duration);
  }

  showInfo(message: string, duration = 4000) {
    this.push(message, 'info', duration);
  }

  dismiss(id: number) {
    this._toasts.update(list => list.filter(t => t.id !== id));
  }

  private push(text: string, type: ToastMessage['type'], duration: number) {
    const id = this.nextId++;
    this._toasts.update(list => [...list, { id, text, type }]);
    setTimeout(() => this.dismiss(id), duration);
  }
}
