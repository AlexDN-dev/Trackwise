import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

const UPPERCASE_RE = /.*[A-Z].*/;
const SYMBOL_RE = /.*[^A-Za-z0-9\s].*/;

@Component({
  selector: 'app-auth',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './auth.html',
  styleUrl: './auth.css',
})
export class Auth {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  readonly isLogin = signal(true);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly infoMessage = signal<string | null>(null);
  readonly emailNotConfirmed = signal(false);
  readonly resending = signal(false);

  readonly form = this.fb.nonNullable.group({
    firstname: [''],
    lastname: [''],
    email: ['', [Validators.required, Validators.email]],
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

  readonly firstnameOk = computed(() => this.fieldValid('firstname'));
  readonly lastnameOk = computed(() => this.fieldValid('lastname'));
  readonly emailOk = computed(() => this.fieldValid('email'));
  readonly passwordOk = computed(() => this.fieldValid('password'));

  readonly passwordHint = computed(() => {
    this.value();
    const v = this.form.controls.password.value ?? '';
    if (!v) return '8 caractères, 1 majuscule, 1 symbole';
    const score = [v.length >= 8, /[A-Z]/.test(v), /[^A-Za-z0-9]/.test(v)].filter(Boolean).length;
    return ['Faible', 'Faible', 'Moyen', 'Fort'][score];
  });

  readonly canSubmit = computed(() => {
    this.value();
    this.isLogin();
    return this.form.valid && !this.loading();
  });

  private fieldValid(name: 'firstname' | 'lastname' | 'email' | 'password'): boolean {
    this.value();
    const c = this.form.controls[name];
    return c.valid && c.value.trim().length > 0;
  }

  setMode(login: boolean): void {
    if (this.isLogin() === login) return;
    this.isLogin.set(login);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    this.emailNotConfirmed.set(false);

    const nameValidators = login ? [] : [Validators.required];
    this.form.controls.firstname.setValidators(nameValidators);
    this.form.controls.lastname.setValidators(nameValidators);
    this.form.controls.firstname.updateValueAndValidity();
    this.form.controls.lastname.updateValueAndValidity();

    const pwValidators = login
      ? [Validators.required, Validators.maxLength(100)]
      : [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(100),
          Validators.pattern(UPPERCASE_RE),
          Validators.pattern(SYMBOL_RE),
        ];
    this.form.controls.password.setValidators(pwValidators);
    this.form.controls.password.updateValueAndValidity();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);
    this.emailNotConfirmed.set(false);

    const { email, password, firstname, lastname } = this.form.getRawValue();

    if (this.isLogin()) {
      this.auth.login({ email, password }).subscribe({
        next: () => this.router.navigate(['/candidatures']),
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          if (err.status === 403 && err.error?.code === 'EMAIL_NOT_CONFIRMED') {
            this.emailNotConfirmed.set(true);
          } else {
            this.errorMessage.set('Email ou mot de passe incorrect.');
          }
        },
      });
    } else {
      this.auth.register({ firstname, lastname, email, password }).subscribe({
        next: () => {
          this.setMode(true);
          this.form.controls.password.reset();
          this.loading.set(false);
          this.infoMessage.set(
            'Compte créé ! Vérifie ta boîte mail et clique sur le lien de confirmation pour activer ton compte.',
          );
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Impossible de créer le compte. Vérifie tes informations.');
        },
      });
    }
  }

  resend(): void {
    const email = this.form.controls.email.value;
    if (!email) {
      this.errorMessage.set('Renseigne ton adresse email pour recevoir un nouveau lien.');
      return;
    }
    this.resending.set(true);
    // Réponse volontairement générique (anti-énumération) : même message en cas d'erreur.
    this.auth.resendConfirmation(email).subscribe({
      next: () => this.afterResend(),
      error: () => this.afterResend(),
    });
  }

  private afterResend(): void {
    this.resending.set(false);
    this.toastService.success('Si un compte non confirmé existe, un nouvel email vient d’être envoyé.');
  }
}
