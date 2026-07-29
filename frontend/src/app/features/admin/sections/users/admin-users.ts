import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import {
  AdminUserDetail,
  AdminUserListItem,
  AdminUsersPage,
  UserRole,
} from '../../../../core/models/admin.models';
import { PlanType } from '../../../../core/models/user.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-users',
  imports: [FormsModule],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsers implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly UserRole = UserRole;
  readonly PlanType = PlanType;

  readonly page = signal<AdminUsersPage | null>(null);
  readonly currentPage = signal(1);
  readonly pageSize = signal(20);
  readonly search = signal('');
  readonly loading = signal(true);

  readonly selected = signal<AdminUserDetail | null>(null);
  readonly modalLoading = signal(false);
  readonly trialDays = signal(30);
  readonly grantCount = signal(5);

  readonly totalPages = computed(() => {
    const p = this.page();
    if (!p) return 1;
    return Math.max(1, Math.ceil(p.total / p.size));
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.adminService
      .listUsers(this.currentPage(), this.pageSize(), this.search())
      .subscribe({
        next: (p) => {
          this.page.set(p);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.currentPage.set(1);
    this.reload();
  }

  clearSearch(): void {
    this.search.set('');
    this.currentPage.set(1);
    this.reload();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages()) return;
    this.currentPage.set(p);
    this.reload();
  }

  openDetail(user: AdminUserListItem): void {
    this.modalLoading.set(true);
    this.adminService.getUser(user.id).subscribe({
      next: (u) => {
        this.selected.set(u);
        this.modalLoading.set(false);
      },
      error: () => {
        this.modalLoading.set(false);
        this.toast.error('Impossible de charger les détails.');
      },
    });
  }

  closeDetail(): void {
    this.selected.set(null);
  }

  ban(): void {
    const u = this.selected();
    if (!u) return;
    this.adminService.banUser(u.id).subscribe({
      next: () => {
        this.toast.success('Utilisateur banni.');
        this.selected.update((s) => (s ? { ...s, isBanned: true } : null));
        this.reload();
      },
      error: () => this.toast.error('Impossible de bannir.'),
    });
  }

  unban(): void {
    const u = this.selected();
    if (!u) return;
    this.adminService.unbanUser(u.id).subscribe({
      next: () => {
        this.toast.success('Utilisateur débanni.');
        this.selected.update((s) => (s ? { ...s, isBanned: false } : null));
        this.reload();
      },
      error: () => this.toast.error('Impossible de débannir.'),
    });
  }

  grantTrial(): void {
    const u = this.selected();
    if (!u) return;
    const days = this.trialDays();
    if (days < 1 || days > 365) {
      this.toast.error('Entre 1 et 365 jours.');
      return;
    }
    this.adminService.grantTrial(u.id, days).subscribe({
      next: () => {
        this.toast.success(`Trial de ${days} jours accordé.`);
        this.openDetail(u);
        this.reload();
      },
      error: () => this.toast.error("Impossible d'accorder le trial."),
    });
  }

  grantTokens(): void {
    const u = this.selected();
    if (!u) return;
    const count = this.grantCount();
    if (count < 1 || count > 1000) {
      this.toast.error('Entre 1 et 1000.');
      return;
    }
    this.adminService.grantTokens(u.id, count).subscribe({
      next: () => {
        this.toast.success(`+${count} analyses accordées.`);
        this.openDetail(u);
      },
      error: () => this.toast.error("Impossible d'accorder les tokens."),
    });
  }

  formatDate(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }
}
