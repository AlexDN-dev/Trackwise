import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { LogsResponse } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-admin-logs',
  imports: [FormsModule],
  templateUrl: './admin-logs.html',
  styleUrl: './admin-logs.css',
})
export class AdminLogs implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly logs = signal<LogsResponse | null>(null);
  readonly loading = signal(true);

  readonly linesCount = signal(200);
  readonly level = signal<string>('');
  readonly search = signal('');

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.adminService
      .logs(this.linesCount(), this.level() || undefined, this.search() || undefined)
      .subscribe({
        next: (r) => {
          this.logs.set(r);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  levelClass(level: string | null): string {
    if (!level) return '';
    return level.toLowerCase();
  }
}
