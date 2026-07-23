import { Component, ElementRef, inject, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CandidatureService } from '../../../../core/services/candidature.service';
import { CandidatureResponse, ContractType } from '../../../../core/models/candidature.models';

@Component({
  selector: 'app-candidature-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './candidature-modal.html',
  styleUrl: './candidature-modal.css',
})
export class CandidatureModal {
  private readonly fb = inject(FormBuilder);
  private readonly candidatureService = inject(CandidatureService);

  readonly close = output<void>();
  readonly saved = output<CandidatureResponse>();

  readonly mode = signal<'manual' | 'ai'>('manual');
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly contractTypes = [
    { value: ContractType.CDI, label: 'CDI'},
    { value: ContractType.CDD, label: 'CDD'},
    { value: ContractType.Interim, label: 'Interim'},
  ];

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required, Validators.maxLength(100)],
    entrepriseName: ['', Validators.required, Validators.maxLength(150)],
    location: [''],
    contractType: [ContractType.CDI, Validators.required],
    urlOffer: [''],
    description: [''],
  });

  setMode(mode: 'manual' | 'ai'): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
  }

  onOverlayClick(event: MouseEvent): void {
    if(event.target === event.currentTarget) this.close.emit();
  }

  submit(): void {
    if(this.form.invalid) {
      this.form.markAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    this.candidatureService.create({
      title: raw.title,
      entrepriseName: raw.entrepriseName,
      location: raw.location || null,
      contractType: raw.contractType,
      urlOffer: raw.urlOffer || null,
      description: raw.description || null,
    })
      .subscribe({
        next: (created) => {
          this.saved.emit(created);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set("Impossible d'ajouter la candidature.");
        }
      });
  }
}
