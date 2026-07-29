import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminService } from '../../../../core/services/admin.service';
import { AdminStatsResponse } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-admin-stats',
  imports: [],
  templateUrl: './admin-stats.html',
  styleUrl: './admin-stats.css',
})
export class AdminStats implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly stats = signal<AdminStatsResponse | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.adminService.stats().subscribe({
      next: (s) => {
        this.stats.set(s);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
