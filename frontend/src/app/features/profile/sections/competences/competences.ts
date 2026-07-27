import { Component, computed, ElementRef, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Competence {
  id: string;
  name: string;
}

// Suggestions mockées — à remplacer par GET /api/competences?search=xxx
const MOCK_SUGGESTIONS = [
  'C#', '.NET', 'ASP.NET Core', 'Entity Framework', 'SQL Server', 'PostgreSQL',
  'Angular', 'TypeScript', 'JavaScript', 'RxJS', 'Signals', 'React', 'Vue.js',
  'Node.js', 'Python', 'Java', 'Kotlin', 'Go', 'Rust', 'PHP',
  'Docker', 'Kubernetes', 'AWS', 'Azure', 'Git', 'CI/CD', 'GitHub Actions',
  'REST', 'GraphQL', 'gRPC', 'Redis', 'MongoDB', 'ElasticSearch',
];

@Component({
  selector: 'app-profile-competences',
  imports: [FormsModule],
  templateUrl: './competences.html',
  styleUrl: './competences.css',
})
export class ProfileCompetences {
  private readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('typeahead');

  // Placeholder initial — à brancher sur GET /api/users/me/competences
  readonly competences = signal<Competence[]>([
    { id: '1', name: 'C#' },
    { id: '2', name: '.NET' },
    { id: '3', name: 'Angular' },
    { id: '4', name: 'TypeScript' },
    { id: '5', name: 'Docker' },
  ]);

  readonly query = signal('');
  readonly dropdownOpen = signal(false);

  private norm(s: string): string {
    return s.toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '');
  }

  readonly suggestions = computed(() => {
    const q = this.norm(this.query().trim());
    if (!q) return [];
    const ownedNorm = new Set(this.competences().map((c) => this.norm(c.name)));
    return MOCK_SUGGESTIONS
      .filter((s) => this.norm(s).includes(q) && !ownedNorm.has(this.norm(s)))
      .slice(0, 6);
  });

  readonly canCreate = computed(() => {
    const q = this.query().trim();
    if (!q) return false;
    const qn = this.norm(q);
    const ownedNorm = new Set(this.competences().map((c) => this.norm(c.name)));
    if (ownedNorm.has(qn)) return false;
    const exactMatch = this.suggestions().some((s) => this.norm(s) === qn);
    return !exactMatch;
  });

  addFromName(name: string): void {
    const trimmed = name.trim();
    if (!trimmed) return;
    const qn = this.norm(trimmed);
    if (this.competences().some((c) => this.norm(c.name) === qn)) return;
    // À brancher sur POST /api/users/me/competences avec { name }
    this.competences.update((list) => [
      ...list,
      { id: crypto.randomUUID(), name: trimmed },
    ]);
    this.query.set('');
    this.inputRef()?.nativeElement.focus();
  }

  remove(competence: Competence): void {
    // À brancher sur DELETE /api/users/me/competences/{id}
    this.competences.update((list) => list.filter((c) => c.id !== competence.id));
  }

  onFocus(): void {
    this.dropdownOpen.set(true);
  }

  onBlur(): void {
    // Petit délai pour laisser passer le click sur une suggestion
    setTimeout(() => this.dropdownOpen.set(false), 150);
  }

  onEnter(): void {
    const first = this.suggestions()[0];
    if (first) {
      this.addFromName(first);
    } else if (this.canCreate()) {
      this.addFromName(this.query());
    }
  }
}
