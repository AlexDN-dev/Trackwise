import {
  Component,
  computed,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CandidatureService } from '../../../../core/services/candidature.service';
import { CandidatureResponse, ContractType } from '../../../../core/models/candidature.models';
import { AiAnalysisService } from '../../../../core/services/ai-analysis.service';
import { AnalysisResult } from '../../../../core/models/ai.models';

@Component({
  selector: 'app-candidature-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './candidature-modal.html',
  styleUrl: './candidature-modal.css',
})
export class CandidatureModal implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly candidatureService = inject(CandidatureService);
  private readonly aiService = inject(AiAnalysisService);

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
    } else if (prefill) {
      this.form.patchValue({
        title: prefill.title ?? '',
        entrepriseName: prefill.entrepriseName ?? '',
        location: prefill.location ?? '',
        contractType: prefill.contractType ?? ContractType.CDI,
        urlOffer: prefill.urlOffer ?? '',
        description: prefill.description ?? '',
      });
    }
  }

  setMode(mode: 'manual' | 'ai'): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.close.emit();
  }

  analyzeAi() : void {
    if(!this.canAnalyze()) return;
    this.aiService.analyze(this.aiText().trim());
    this.aiText.set('');
    this.close.emit();
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
        })
      : this.candidatureService.create({
          title: raw.title,
          entrepriseName: raw.entrepriseName,
          location: raw.location || null,
          contractType: raw.contractType,
          urlOffer: raw.urlOffer || null,
          description: raw.description || null,
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
