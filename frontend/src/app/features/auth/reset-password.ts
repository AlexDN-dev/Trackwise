import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

const UPPERCASE_RE = /.*[A-Z].*/;
const SYMBOL_RE = /.*[^A-Za-z0-9\s].*/;

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './auth.css',
})
export class ResetPassword implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  private token = '';
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly tokenMissing = signal(false);

  readonly form = this.fb.nonNullable.group({
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100),
        Validators.pattern(UPPERCASE_RE),
        Validators.pattern(SYMBOL_RE),
      ],
    ],
  });

  private readonly value = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  readonly passwordOk = computed(() => {
    this.value();
    const c = this.form.controls.password;
    return c.valid && c.value.trim().length > 0;
  });

  readonly passwordHint = computed(() => {
    this.value();
    const v = this.form.controls.password.value ?? '';
    if (!v) return '8 caractères, 1 majuscule, 1 symbole';
    const score = [v.length >= 8, /[A-Z]/.test(v), /[^A-Za-z0-9]/.test(v)].filter(Boolean).length;
    return ['Faible', 'Faible', 'Moyen', 'Fort'][score];
  });

  readonly canSubmit = computed(() => {
    this.value();
    return this.form.valid && !this.loading();
  });

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) this.tokenMissing.set(true);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);

    this.auth.resetPassword(this.token, this.form.getRawValue().password).subscribe({
      next: () => {
        this.toast.success('Mot de passe réinitialisé. Connecte-toi avec ton nouveau mot de passe.');
        this.router.navigate(['/auth']);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Ce lien est invalide ou expiré. Demande un nouveau lien.');
      },
    });
  }
}
