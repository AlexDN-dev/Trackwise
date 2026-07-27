import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';

@Component({
  selector: 'app-profile-informations',
  imports: [FormsModule],
  templateUrl: './informations.html',
  styleUrl: './informations.css',
})
export class ProfileInformations implements OnInit {
  private readonly userService = inject(UserService);

  readonly firstname = signal('');
  readonly lastname = signal('');
  readonly email = signal('');
  readonly memberSince = signal('');

  private initialFirstname = '';
  private initialLastname = '';

  readonly initials = computed(
    () => ((this.firstname()[0] ?? '') + (this.lastname()[0] ?? '')).toUpperCase() || '?',
  );
  readonly displayName = computed(() => `${this.firstname()} ${this.lastname()}`.trim());

  readonly hasChanges = computed(
    () =>
      this.firstname().trim() !== this.initialFirstname ||
      this.lastname().trim() !== this.initialLastname,
  );

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.userService.getMe().subscribe({
      next: (user) => {
        this.firstname.set(user.firstname);
        this.lastname.set(user.lastname);
        this.email.set(user.email);
        this.initialFirstname = user.firstname;
        this.initialLastname = user.lastname;
        this.memberSince.set(
          new Date(user.createdAt).toLocaleDateString('fr-FR', { month: 'long', year: 'numeric' }),
        );
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Impossible de charger votre profil.');
        this.loading.set(false);
      },
    });
  }

  save(): void {
    if (!this.hasChanges() || this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set(null);

    this.userService
      .update({ firstname: this.firstname().trim(), lastname: this.lastname().trim() })
      .subscribe({
        next: (user) => {
          this.firstname.set(user.firstname);
          this.lastname.set(user.lastname);
          this.initialFirstname = user.firstname;
          this.initialLastname = user.lastname;
          this.saving.set(false);
        },
        error: () => {
          this.errorMessage.set('Erreur lors de la sauvegarde.');
          this.saving.set(false);
        },
      });
  }

  reset(): void {
    this.firstname.set(this.initialFirstname);
    this.lastname.set(this.initialLastname);
  }
}
