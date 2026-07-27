import {
  Component,
  computed,
  ElementRef,
  inject,
  input,
  OnInit,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, of, Subject, switchMap } from 'rxjs';
import { CandidatureService } from '../../../../core/services/candidature.service';
import { CandidatureResponse, ContractType } from '../../../../core/models/candidature.models';
import { AiAnalysisService } from '../../../../core/services/ai-analysis.service';
import { AnalysisResult } from '../../../../core/models/ai.models';
import { CompetenceService } from '../../../../core/services/competence.service';
import { CompetenceResponse } from '../../../../core/models/competence.models';

@Component({
  selector: 'app-candidature-modal',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './candidature-modal.html',
  styleUrl: './candidature-modal.css',
})
export class CandidatureModal implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly candidatureService = inject(CandidatureService);
  private readonly aiService = inject(AiAnalysisService);
  private readonly competenceService = inject(CompetenceService);
  private readonly skillInputRef = viewChild<ElementRef<HTMLInputElement>>('skillInput');

  readonly prefill = input<AnalysisResult | null>(null);
  readonly isPrefill = computed(() => this.prefill() !== null);
  readonly showTabs = computed(() => !this.isEdit() && !this.isPrefill());

  readonly aiText = signal('');
  readonly canAnalyze = computed(() => this.aiText().trim().length >= 50);

  readonly close = output<void>();
  readonly saved = output<CandidatureResponse>();
  readonly existing = input<CandidatureResponse | null>(null);
  readonly isEdit = computed(() => this.existing() !== null);

  readonly mode = signal<'manual' | 'ai'>('manual');
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly competences = signal<string[]>([]);
  readonly skillQuery = signal('');
  readonly skillSuggestions = signal<CompetenceResponse[]>([]);
  readonly skillDropdownOpen = signal(false);

  private readonly skillQuerySubject = new Subject<string>();

  private norm(s: string): string {
    return s.toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '');
  }

  readonly canCreateSkill = computed(() => {
    const q = this.skillQuery().trim();
    if (!q) return false;
    const qn = this.norm(q);
    const owned = new Set(this.competences().map((c) => this.norm(c)));
    if (owned.has(qn)) return false;
    return !this.skillSuggestions().some((s) => this.norm(s.name) === qn);
  });

  readonly contractTypes = [
    { value: ContractType.CDI, label: 'CDI' },
    { value: ContractType.CDD, label: 'CDD' },
    { value: ContractType.Interim, label: 'Interim' },
  ];

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    entrepriseName: ['', [Validators.required, Validators.maxLength(150)]],
    location: [''],
    contractType: [ContractType.CDI, [Validators.required]],
    urlOffer: [''],
    description: [''],
  });

  ngOnInit(): void {
    const existing = this.existing();
    const prefill = this.prefill();

    if (existing) {
      this.form.patchValue({
        title: existing.title,
        entrepriseName: existing.entrepriseName ?? '',
        location: existing.location ?? '',
        contractType: existing.contractType,
        urlOffer: existing.urlOffer ?? '',
        description: existing.description ?? '',
      });
      this.competences.set(existing.competences?.map((c) => c.name) ?? []);
    } else if (prefill) {
      this.form.patchValue({
        title: prefill.title ?? '',
        entrepriseName: prefill.entrepriseName ?? '',
        location: prefill.location ?? '',
        contractType: prefill.contractType ?? ContractType.CDI,
        urlOffer: prefill.urlOffer ?? '',
        description: prefill.description ?? '',
      });
      this.competences.set(prefill.competences ?? []);
    }

    this.skillQuerySubject
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        switchMap((term) => (term.trim() ? this.competenceService.search(term.trim()) : of([]))),
      )
      .subscribe({
        next: (results) => {
          const owned = new Set(this.competences().map((c) => this.norm(c)));
          this.skillSuggestions.set(results.filter((r) => !owned.has(this.norm(r.name))));
        },
      });
  }

  setMode(mode: 'manual' | 'ai'): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.close.emit();
  }

  analyzeAi(): void {
    if (!this.canAnalyze()) return;
    this.aiService.analyze(this.aiText().trim());
    this.aiText.set('');
    this.close.emit();
  }

  onSkillQueryChange(value: string): void {
    this.skillQuery.set(value);
    this.skillQuerySubject.next(value);
  }

  addSkill(name: string): void {
    const trimmed = name.trim();
    if (!trimmed) return;
    const n = this.norm(trimmed);
    if (this.competences().some((c) => this.norm(c) === n)) return;
    this.competences.update((list) => [...list, trimmed]);
    this.skillQuery.set('');
    this.skillSuggestions.set([]);
    this.skillInputRef()?.nativeElement.focus();
  }

  removeSkill(name: string): void {
    this.competences.update((list) => list.filter((c) => c !== name));
  }

  onSkillFocus(): void {
    this.skillDropdownOpen.set(true);
  }

  onSkillBlur(): void {
    setTimeout(() => this.skillDropdownOpen.set(false), 150);
  }

  onSkillEnter(): void {
    const first = this.skillSuggestions()[0];
    if (first) this.addSkill(first.name);
    else if (this.canCreateSkill()) this.addSkill(this.skillQuery());
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    const existing = this.existing();
    const competences = this.competences();

    const request$ = existing
      ? this.candidatureService.update(existing.id, {
          title: raw.title,
          location: raw.location || null,
          urlOffer: raw.urlOffer || null,
          description: raw.description || null,
          contractType: raw.contractType,
          status: existing.status,
          postulationDate: existing.postulationDate,
          entrepriseId: existing.entrepriseId,
          competences,
        })
      : this.candidatureService.create({
          title: raw.title,
          entrepriseName: raw.entrepriseName,
          location: raw.location || null,
          contractType: raw.contractType,
          urlOffer: raw.urlOffer || null,
          description: raw.description || null,
          competences,
        });

    request$.subscribe({
      next: (result) => {
        this.saved.emit(result);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set(
          existing
            ? 'Impossible de modifier la candidature.'
            : "Impossible d'ajouter la candidature.",
        );
      },
    });
  }
}
