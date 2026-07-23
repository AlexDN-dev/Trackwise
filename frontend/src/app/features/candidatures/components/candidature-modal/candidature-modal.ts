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

@Component({
  selector: 'app-candidature-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './candidature-modal.html',
  styleUrl: './candidature-modal.css',
})
export class CandidatureModal implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly candidatureService = inject(CandidatureService);

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
    const c = this.existing();
    if (c) {
      this.form.patchValue({
        title: c.title,
        entrepriseName: c.entrepriseName ?? '',
        location: c.location ?? '',
        contractType: c.contractType,
        urlOffer: c.urlOffer ?? '',
        description: c.description ?? '',
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
