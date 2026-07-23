import { Component, HostListener, computed, input, output, signal } from '@angular/core';
import { CandidatureResponse } from '../../../../core/models/candidature.models';

const LOGO_CLASSES = ['lg1', 'lg2', 'lg3', 'lg4', 'lg5', 'lg6', 'lg7', 'lg8'];

@Component({
  selector: 'app-kanban-card',
  imports: [],
  templateUrl: './kanban-card.html',
  styleUrl: './kanban-card.css',
  host: { class: 'kcard' },
})
export class KanbanCard {
  readonly card = input.required<CandidatureResponse>();
  readonly canMoveLeft = input(false);
  readonly canMoveRight = input(false);
  readonly edit = output<CandidatureResponse>();
  readonly delete = output<CandidatureResponse>();
  readonly moveLeft = output<CandidatureResponse>();
  readonly moveRight = output<CandidatureResponse>();

  readonly menuOpen = signal(false);

  readonly initial = computed(() => (this.card().entrepriseName ?? '?').charAt(0).toUpperCase());

  readonly logoCls = computed(() => {
    const id = this.card().entrepriseId ?? '';
    const idx = [...id].reduce((sum, c) => sum + c.charCodeAt(0), 0) % LOGO_CLASSES.length;
    return LOGO_CLASSES[idx];
  });

  readonly date = computed(() => {
    const iso = this.card().createdAt;
    return iso ? new Date(iso).toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }) : '';
  });

  toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.update((v) => !v);
  }

  @HostListener('document:click')
  closeMenu(): void {
    this.menuOpen.set(false);
  }

  onEdit(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.set(false);
    this.edit.emit(this.card());
  }

  onDelete(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.set(false);
    this.delete.emit(this.card());
  }

  onMoveLeft(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.set(false);
    this.moveLeft.emit(this.card());
  }

  onMoveRight(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.set(false);
    this.moveRight.emit(this.card());
  }
}
