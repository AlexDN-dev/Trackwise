import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile-informations',
  imports: [FormsModule],
  templateUrl: './informations.html',
  styleUrl: './informations.css',
})
export class ProfileInformations {
  // Placeholders — à brancher sur GET /api/users/me
  readonly email = signal('marie.dupont@exemple.fr');
  readonly emailConfirmed = signal(false);
  readonly memberSince = signal('juillet 2026');

  readonly initialDisplayName = signal('Marie Dupont');
  readonly displayName = signal('Marie Dupont');

  readonly initials = computed(() => {
    const parts = this.displayName().trim().split(/\s+/);
    return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '');
  });

  readonly hasChanges = computed(() => this.displayName().trim() !== this.initialDisplayName());
  readonly saving = signal(false);

  save(): void {
    // À brancher sur PATCH /api/users/me
    this.saving.set(true);
    setTimeout(() => {
      this.initialDisplayName.set(this.displayName().trim());
      this.saving.set(false);
    }, 400);
  }

  reset(): void {
    this.displayName.set(this.initialDisplayName());
  }
}
