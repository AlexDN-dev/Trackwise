import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-auth',
  imports: [ReactiveFormsModule],
  templateUrl: './auth.html',
  styleUrl: './auth.css',
})
export class Auth {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLogin = signal(true);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstname: [''],
    lastname: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  // ticke à chaque frappe → recalcul des états dérivés (compatible zoneless)
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

    const req = login ? [] : [Validators.required];
    this.form.controls.firstname.setValidators(req);
    this.form.controls.lastname.setValidators(req);
    this.form.controls.firstname.updateValueAndValidity();
    this.form.controls.lastname.updateValueAndValidity();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);

    const { email, password, firstname, lastname } = this.form.getRawValue();
    const request$ = this.isLogin()
      ? this.auth.login({ email, password })
      : this.auth.register({ firstname, lastname, email, password });

    request$.subscribe({
      next: () => this.router.navigate(['/candidatures']),
      error: () => {
        this.loading.set(false);
        this.errorMessage.set(
          this.isLogin()
            ? 'Email ou mot de passe incorrect.'
            : 'Impossible de créer le compte. Vérifie tes informations.',
        );
      },
    });
  }
}
