import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile-security',
  imports: [FormsModule],
  templateUrl: './security.html',
  styleUrl: './security.css',
})
export class ProfileSecurity {
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
    // À brancher sur POST /api/users/me/change-password
    setTimeout(() => {
      this.saving.set(false);
      this.successMessage.set('Mot de passe mis à jour. Vos autres sessions ont été déconnectées.');
      this.currentPassword.set('');
      this.newPassword.set('');
      this.confirmPassword.set('');
    }, 600);
  }
}
