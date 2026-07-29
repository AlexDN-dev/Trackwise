import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { BannedIp } from '../../../../core/models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-ips',
  imports: [FormsModule],
  templateUrl: './admin-ips.html',
  styleUrl: './admin-ips.css',
})
export class AdminIps implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly ips = signal<BannedIp[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);

  readonly formIp = signal('');
  readonly formReason = signal('');

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.adminService.listIps().subscribe({
      next: (list) => {
        this.ips.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  submit(): void {
    const ip = this.formIp().trim();
    if (!ip) {
      this.toast.error('IP requise.');
      return;
    }
    this.submitting.set(true);
    this.adminService.banIp(ip, this.formReason().trim() || null).subscribe({
      next: () => {
        this.toast.success('IP bannie.');
        this.formIp.set('');
        this.formReason.set('');
        this.submitting.set(false);
        this.reload();
      },
      error: () => {
        this.toast.error('Impossible de bannir cette IP.');
        this.submitting.set(false);
      },
    });
  }

  unban(ip: string): void {
    this.adminService.unbanIp(ip).subscribe({
      next: () => {
        this.toast.success('IP débannie.');
        this.reload();
      },
      error: () => this.toast.error('Impossible de débannir.'),
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
