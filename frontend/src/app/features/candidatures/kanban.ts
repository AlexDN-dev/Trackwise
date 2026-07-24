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
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';
import { effect } from '@angular/core';
import { AiAnalysisService} from '../../core/services/ai-analysis.service';
import { AnalysisResult} from '../../core/models/ai.models';

const COLUMNS = [
  { status: CandidatureStatus.ToSend, label: 'À postuler', cls: 'c-postuler' },
  { status: CandidatureStatus.Sent, label: 'Candidature envoyée', cls: 'c-envoyee' },
  { status: CandidatureStatus.GotInterview, label: 'Entretien', cls: 'c-entretien' },
  { status: CandidatureStatus.PositiveResponse, label: 'Réponse positive', cls: 'c-positive' },
  { status: CandidatureStatus.NegativeResponse, label: 'Réponse négative', cls: 'c-negative' },
];

const STATUS_ORDER = COLUMNS.map((c) => c.status);

@Component({
  selector: 'app-kanban',
  imports: [KanbanCard, CdkDropListGroup, CdkDropList, CdkDrag, CandidatureModal, ConfirmDialog],
  templateUrl: './kanban.html',
  styleUrl: './kanban.css',
})
export class Kanban {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly candidatureService = inject(CandidatureService);
  private readonly aiService  = inject(AiAnalysisService);

  readonly candidatures = signal<CandidatureResponse[]>([]);
  readonly loading = signal(true);
  readonly total = computed(() => this.candidatures().length);
  readonly isEmpty = computed(() => !this.loading() && this.total() === 0);
  readonly modalOpen = signal(false);
  readonly editingCandidature = signal<CandidatureResponse | null>(null);
  readonly pendingDelete = signal<CandidatureResponse | null>(null);
  readonly aiPrefill = signal<AnalysisResult | null>(null);

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

    effect(() => {
      const jobId = this.aiService.openRequest();
      if (!jobId) return;
      const job = this.aiService.readyJob();
      if (!job || !job.result) return;

      this.aiPrefill.set(job.result);
      this.editingCandidature.set(null);
      this.modalOpen.set(true);

      this.aiService.clearOpenRequest();
      this.aiService.consume(jobId);
    });
  }

  toggleColumn(status: CandidatureStatus): void {
    this.collapsed.update((set) => {
      const next = new Set(set);
      next.has(status) ? next.delete(status) : next.add(status);
      return next;
    });
  }

  canMoveLeft(status: CandidatureStatus): boolean {
    return STATUS_ORDER.indexOf(status) > 0;
  }

  canMoveRight(status: CandidatureStatus): boolean {
    return STATUS_ORDER.indexOf(status) < STATUS_ORDER.length - 1;
  }

  drop(event: CdkDragDrop<CandidatureResponse[]>, newStatus: CandidatureStatus): void {
    if (event.previousContainer === event.container) return;
    const card = event.previousContainer.data[event.previousIndex];
    this.persistStatus(card, newStatus);
  }

  moveCard(card: CandidatureResponse, direction: -1 | 1): void {
    const target = STATUS_ORDER[STATUS_ORDER.indexOf(card.status) + direction];
    if (target !== undefined) this.persistStatus(card, target);
  }

  private persistStatus(card: CandidatureResponse, newStatus: CandidatureStatus): void {
    const oldStatus = card.status;

    this.candidatures.update((list) =>
      list.map((c) => (c.id === card.id ? { ...c, status: newStatus } : c)),
    );

    this.candidatureService
      .update(card.id, {
        title: card.title,
        location: card.location,
        urlOffer: card.urlOffer,
        description: card.description,
        contractType: card.contractType,
        status: newStatus,
        postulationDate: card.postulationDate,
        entrepriseId: card.entrepriseId,
      })
      .subscribe({
        error: () =>
          this.candidatures.update((list) =>
            list.map((c) => (c.id === card.id ? { ...c, status: oldStatus } : c)),
          ),
      });
  }

  openAddModal(): void {
    this.editingCandidature.set(null);
    this.aiPrefill.set(null);
    this.modalOpen.set(true);
  }

  openEditModal(candidature: CandidatureResponse): void {
    this.editingCandidature.set(candidature);
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
    this.editingCandidature.set(null);
    this.aiPrefill.set(null);
  }

  onCandidatureSaved(saved: CandidatureResponse): void {
    const isEdit = this.editingCandidature() !== null;
    this.candidatures.update((list) =>
      isEdit ? list.map((c) => (c.id === saved.id ? saved : c)) : [...list, saved],
    );
    this.closeModal();
  }

  requestDelete(candidature: CandidatureResponse): void {
    this.pendingDelete.set(candidature);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const candidature = this.pendingDelete();
    if (!candidature) return;

    const previous = this.candidatures();
    // Optimistic UI
    this.candidatures.update((list) => list.filter((c) => c.id !== candidature.id));
    this.pendingDelete.set(null);

    this.candidatureService.delete(candidature.id).subscribe({
      error: () => this.candidatures.set(previous),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth']);
  }
}
