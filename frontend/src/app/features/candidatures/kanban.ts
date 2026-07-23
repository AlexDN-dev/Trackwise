import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AuthService } from '../../core/services/auth.service';
import { CandidatureService } from '../../core/services/candidature.service';
import {
  CandidatureResponse,
  CandidatureStatus,
} from '../../core/models/candidature.models';
import { KanbanCard } from './components/kanban-card/kanban-card';
import { CandidatureModal } from './components/candidature-modal/candidature-modal';

const COLUMNS = [
  { status: CandidatureStatus.ToSend, label: 'À postuler', cls: 'c-postuler' },
  { status: CandidatureStatus.Sent, label: 'Candidature envoyée', cls: 'c-envoyee' },
  { status: CandidatureStatus.GotInterview, label: 'Entretien', cls: 'c-entretien' },
  { status: CandidatureStatus.PositiveResponse, label: 'Réponse positive', cls: 'c-positive' },
  { status: CandidatureStatus.NegativeResponse, label: 'Réponse négative', cls: 'c-negative' },
];

@Component({
  selector: 'app-kanban',
  imports: [KanbanCard, CdkDropListGroup, CdkDropList, CdkDrag, CandidatureModal],
  templateUrl: './kanban.html',
  styleUrl: './kanban.css',
})
export class Kanban {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly candidatureService = inject(CandidatureService);

  readonly candidatures = signal<CandidatureResponse[]>([]);
  readonly loading = signal(true);
  readonly total = computed(() => this.candidatures().length);
  readonly isEmpty = computed(() => !this.loading() && this.total() === 0);
  readonly modalOpen = signal(false);

  private readonly collapsed = signal<Set<CandidatureStatus>>(new Set());
  isCollapsed = (status: CandidatureStatus) => this.collapsed().has(status);

  readonly columns = computed(() =>
    COLUMNS.map((col) => ({
      ...col,
      cards: this.candidatures().filter((c) => c.status === col.status),
    })),
  );

  constructor() {
    this.candidatureService.getAll().subscribe({
      next: (list) => {
        this.candidatures.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleColumn(status: CandidatureStatus): void {
    this.collapsed.update((set) => {
      const next = new Set(set);
      next.has(status) ? next.delete(status) : next.add(status);
      return next;
    });
  }

  // TODO: appeler CandidatureService.update(id, { ..., status: newStatus }) pour persister
  drop(event: CdkDragDrop<CandidatureResponse[]>, newStatus: CandidatureStatus): void {
    if (event.previousContainer === event.container) return;

    const cardId = event.previousContainer.data[event.previousIndex].id;
    this.candidatures.update((list) =>
      list.map((c) => (c.id === cardId ? { ...c, status: newStatus } : c)),
    );
  }

  // TODO: brancher l'ouverture du modal Ajouter une candidature
  openAddModal(): void {
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  onCandidatureSaved(created: CandidatureResponse): void {
    this.candidatures.update((list) => [...list, created]);
    this.modalOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth']);
  }
}
