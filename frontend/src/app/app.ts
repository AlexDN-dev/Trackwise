import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { AiJobBadge } from './shared/components/ai-job-badge/ai-job-badge';
import { Toaster } from './shared/components/toaster/toaster';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule, InputTextModule, AiJobBadge, Toaster],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('frontend');
}
