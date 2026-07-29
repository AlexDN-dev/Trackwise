import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminService } from '../../../../core/services/admin.service';
import { AdminAuditLog } from '../../../../core/models/admin.models';

const ACTION_LABELS: Record<string, { label: string; kind: string }> = {
  BanUser: { label: 'Ban utilisateur', kind: 'danger' },
  UnbanUser: { label: 'Unban utilisateur', kind: 'ok' },
  GrantTrial: { label: 'Trial accordé', kind: 'accent' },
  GrantTokens: { label: 'Tokens accordés', kind: 'accent' },
  BanIp: { label: 'Ban IP', kind: 'danger' },
  UnbanIp: { label: 'Unban IP', kind: 'ok' },
};

@Component({
  selector: 'app-admin-audit',
  imports: [],
  templateUrl: './admin-audit.html',
  styleUrl: './admin-audit.css',
})
export class AdminAudit implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly logs = signal<AdminAuditLog[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.adminService.audit(200).subscribe({
      next: (list) => {
        this.logs.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  actionLabel(action: string): string {
    return ACTION_LABELS[action]?.label ?? action;
  }

  actionKind(action: string): string {
    return ACTION_LABELS[action]?.kind ?? 'neutral';
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  }

  shortId(id: string | null): string {
    if (!id) return '—';
    return id.split('-')[0];
  }

  formatDetails(details: string | null): string {
    if (!details) return '';
    try {
      const obj = JSON.parse(details);
      return Object.entries(obj)
        .map(([k, v]) => `${k}: ${v}`)
        .join(' · ');
    } catch {
      return details;
    }
  }
}
