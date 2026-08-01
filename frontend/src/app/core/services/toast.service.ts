import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface ToastAction {
  label: string;
  run: () => void;
}

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
  action?: ToastAction;
}

export interface ToastOptions {
  durationMs?: number;
  action?: ToastAction;
}

const DEFAULT_DURATION_MS = 4000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  success(message: string, opts?: ToastOptions): void {
    this.push('success', message, opts);
  }

  error(message: string, opts?: ToastOptions): void {
    this.push('error', message, opts);
  }

  info(message: string, opts?: ToastOptions): void {
    this.push('info', message, opts);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  runAction(id: number): void {
    const toast = this.toasts().find((t) => t.id === id);
    toast?.action?.run();
    this.dismiss(id);
  }

  private push(type: ToastType, message: string, opts?: ToastOptions): void {
    const id = this.nextId++;
    const duration = opts?.durationMs ?? (opts?.action ? 6000 : DEFAULT_DURATION_MS);
    this.toasts.update((list) => [...list, { id, type, message, action: opts?.action }]);
    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }
  }
}
