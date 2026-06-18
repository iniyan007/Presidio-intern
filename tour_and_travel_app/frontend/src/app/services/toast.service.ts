import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
  closing?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts = signal<Toast[]>([]);
  private idCounter = 0;

  show(message: string, type: 'success' | 'error' | 'info' = 'info') {
    const id = ++this.idCounter;
    this.toasts.update(current => [...current, { id, message, type }]);

    // Auto dismiss after 3 seconds
    setTimeout(() => this.startClose(id), 2700);
  }

  startClose(id: number) {
    this.toasts.update(current => current.map(t => t.id === id ? { ...t, closing: true } : t));
    setTimeout(() => {
      this.toasts.update(current => current.filter(t => t.id !== id));
    }, 300);
  }

  remove(id: number) {
    const toast = this.toasts().find(t => t.id === id);
    if (toast && !toast.closing) {
      this.startClose(id);
    }
  }
}
