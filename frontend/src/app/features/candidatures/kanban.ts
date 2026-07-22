import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-kanban',
  imports: [ButtonModule],
  template: `
    <div style="padding: 2rem;">
      <h1>Mes candidatures</h1>
      <p>Kanban à construire — tu es bien connecté. 🎉</p>
      <button pButton (click)="logout()">Se déconnecter</button>
    </div>
  `,
})
export class Kanban {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth']);
  }
}
