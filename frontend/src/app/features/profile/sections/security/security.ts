import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../../core/services/toast.service';
import { UserService } from '../../../../core/services/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile-security',
  imports: [FormsModule],
  templateUrl: './security.html',
  styleUrl: './security.css',
})
export class ProfileSecurity {
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  readonly currentPassword = signal('');
  readonly newPassword = signal('');
  readonly confirmPassword = signal('');

  readonly showCurrent = signal(false);
  readonly showNew = signal(false);
  readonly showConfirm = signal(false);

  readonly saving = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly strength = computed(() => {
    const pw = this.newPassword();
    if (!pw) return 0;
    let score = 0;
    if (pw.length >= 8) score++;
    if (pw.length >= 12) score++;
    if (/\d/.test(pw)) score++;
    if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return Math.min(score, 4);
  });

  readonly strengthLabel = computed(() => {
    const s = this.strength();
    if (s === 0) return '';
    if (s <= 1) return 'Faible';
    if (s === 2) return 'Moyen';
    if (s === 3) return 'Bon';
    return 'Excellent';
  });

  readonly canSubmit = computed(() => {
    return (
      this.currentPassword().length > 0 &&
      this.newPassword().length >= 8 &&
      this.newPassword() === this.confirmPassword() &&
      !this.saving()
    );
  });

  readonly matchesError = computed(() => {
    return this.confirmPassword().length > 0 && this.newPassword() !== this.confirmPassword();
  });

  submit(): void {
    if (!this.canSubmit()) return;
    this.saving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.userService.changePassword(this.currentPassword(), this.newPassword()).subscribe({
      next: () => {
        this.toastService.success('Mot de passe mis à jour. Reconnecte-toi pour continuer.');
        this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Impossible de changer le mot de passe.';
        this.errorMessage.set(msg);
        this.toastService.error(msg);
      },
    });
  }
}
