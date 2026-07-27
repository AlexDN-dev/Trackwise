import { Component, computed, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, of, Subject, switchMap } from 'rxjs';
import { CompetenceService } from '../../../../core/services/competence.service';
import { CompetenceResponse } from '../../../../core/models/competence.models';

@Component({
  selector: 'app-profile-competences',
  imports: [FormsModule],
  templateUrl: './competences.html',
  styleUrl: './competences.css',
})
export class ProfileCompetences implements OnInit {
  private readonly service = inject(CompetenceService);
  private readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('typeahead');

  readonly competences = signal<CompetenceResponse[]>([]);
  readonly query = signal('');
  readonly suggestions = signal<CompetenceResponse[]>([]);
  readonly dropdownOpen = signal(false);
  readonly loading = signal(true);
  readonly adding = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private readonly querySubject = new Subject<string>();

  private norm(s: string): string {
    return s.toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '');
  }

  readonly canCreate = computed(() => {
    const q = this.query().trim();
    if (!q) return false;
    const qn = this.norm(q);
    const owned = new Set(this.competences().map((c) => this.norm(c.name)));
    if (owned.has(qn)) return false;
    return !this.suggestions().some((s) => this.norm(s.name) === qn);
  });

  ngOnInit(): void {
    this.service.getMine().subscribe({
      next: (list) => {
        this.competences.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Impossible de charger vos compétences.');
        this.loading.set(false);
      },
    });

    this.querySubject
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        switchMap((term) => (term.trim() ? this.service.search(term.trim()) : of([]))),
      )
      .subscribe({
        next: (results) => {
          const owned = new Set(this.competences().map((c) => this.norm(c.name)));
          this.suggestions.set(results.filter((r) => !owned.has(this.norm(r.name))));
        },
      });
  }

  onQueryChange(value: string): void {
    this.query.set(value);
    this.querySubject.next(value);
  }

  addFromName(name: string): void {
    const trimmed = name.trim();
    if (!trimmed || this.adding()) return;
    this.adding.set(true);
    this.errorMessage.set(null);

    this.service.add({ name: trimmed }).subscribe({
      next: (added) => {
        const owned = new Set(this.competences().map((c) => this.norm(c.name)));
        if (!owned.has(this.norm(added.name))) {
          this.competences.update((list) =>
            [...list, added].sort((a, b) => a.name.localeCompare(b.name)),
          );
        }
        this.query.set('');
        this.suggestions.set([]);
        this.adding.set(false);
        this.inputRef()?.nativeElement.focus();
      },
      error: () => {
        this.errorMessage.set("Impossible d'ajouter cette compétence.");
        this.adding.set(false);
      },
    });
  }

  remove(competence: CompetenceResponse): void {
    const previous = this.competences();
    this.competences.update((list) => list.filter((c) => c.id !== competence.id));

    this.service.remove(competence.id).subscribe({
      error: () => {
        this.competences.set(previous);
        this.errorMessage.set('Impossible de supprimer cette compétence.');
      },
    });
  }

  onFocus(): void {
    this.dropdownOpen.set(true);
  }

  onBlur(): void {
    setTimeout(() => this.dropdownOpen.set(false), 150);
  }

  onEnter(): void {
    const first = this.suggestions()[0];
    if (first) this.addFromName(first.name);
    else if (this.canCreate()) this.addFromName(this.query());
  }
}
