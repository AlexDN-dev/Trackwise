import { Component, computed, input } from '@angular/core';
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

  readonly initial = computed(() =>
    (this.card().entrepriseName ?? '?').charAt(0).toUpperCase(),
  );

  readonly logoCls = computed(() => {
    const id = this.card().entrepriseId ?? '';
    const idx = [...id].reduce((sum, c) => sum + c.charCodeAt(0), 0) % LOGO_CLASSES.length;
    return LOGO_CLASSES[idx];
  });

  readonly date = computed(() => {
    const iso = this.card().createdAt;
    return iso
      ? new Date(iso).toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' })
      : '';
  });
}
