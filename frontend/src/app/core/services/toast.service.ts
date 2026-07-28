import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

const DEFAULT_DURATION_MS = 4000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  success(message: string, durationMs = DEFAULT_DURATION_MS): void {
    this.push('success', message, durationMs);
  }

  error(message: string, durationMs = DEFAULT_DURATION_MS): void {
    this.push('error', message, durationMs);
  }

  info(message: string, durationMs = DEFAULT_DURATION_MS): void {
    this.push('info', message, durationMs);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private push(type: ToastType, message: string, durationMs: number): void {
    const id = this.nextId++;
    this.toasts.update((list) => [...list, { id, type, message }]);
    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }
}
