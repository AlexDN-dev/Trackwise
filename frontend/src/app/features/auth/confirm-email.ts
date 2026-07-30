import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-confirm-email',
  imports: [RouterLink],
  templateUrl: './confirm-email.html',
  styleUrl: './auth.css',
})
export class ConfirmEmail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  readonly state = signal<'loading' | 'success' | 'error'>('loading');

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.state.set('error');
      return;
    }

    this.auth.confirmEmail(token).subscribe({
      next: () => {
        this.state.set('success');
        // Auto-login effectué : petite pause pour afficher la confirmation puis redirection.
        setTimeout(() => this.router.navigate(['/candidatures']), 1400);
      },
      error: () => this.state.set('error'),
    });
  }
}
