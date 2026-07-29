import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly mobileMenuOpen = signal(false);
  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }
  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
