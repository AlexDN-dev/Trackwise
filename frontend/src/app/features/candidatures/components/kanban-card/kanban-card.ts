import { Component, DestroyRef, HostListener, computed, inject, input, output, signal } from '@angular/core';
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
  readonly menuPos = signal<{ top: number; left: number } | null>(null);

  constructor() {
    const onScroll = () => {
      if (this.menuOpen()) this.menuOpen.set(false);
    };
    document.addEventListener('scroll', onScroll, true);
    inject(DestroyRef).onDestroy(() => document.removeEventListener('scroll', onScroll, true));
  }

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

  readonly score = computed(() => this.card().score);

  readonly scoreDisplay = computed(() => {
    const s = this.score();
    return s === null || s === undefined ? '—' : `${s}%`;
  });

  readonly scoreClass = computed(() => {
    const s = this.score();
    if (s === null || s === undefined) return 'sc-none';
    if (s >= 70) return 'sc-hi';
    if (s >= 40) return 'sc-mid';
    return 'sc-lo';
  });

  readonly scoreTooltip = computed(() => {
    const c = this.card();
    if (c.score === null || c.score === undefined) {
      return 'Aucune compétence renseignée sur cette offre';
    }
    const total = c.competences?.length ?? 0;
    const missing = c.missingCompetences ?? [];
    const matched = total - missing.length;
    if (missing.length === 0) return `${matched}/${total} compétences · Match parfait`;
    return `${matched}/${total} compétences · Manque : ${missing.join(', ')}`;
  });

  toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    if (this.menuOpen()) {
      this.menuOpen.set(false);
      return;
    }
    const btn = event.currentTarget as HTMLElement;
    const rect = btn.getBoundingClientRect();
    const menuWidth = 168;
    const left = Math.max(8, rect.right - menuWidth);
    const top = rect.bottom + 4;
    this.menuPos.set({ top, left });
    this.menuOpen.set(true);
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
