import { Component, computed, inject } from '@angular/core';
import { AiAnalysisService } from '../../../core/services/ai-analysis.service';

@Component({
  selector: 'app-ai-job-badge',
  imports: [],
  templateUrl: './ai-job-badge.html',
  styleUrl: './ai-job-badge.css',
})
export class AiJobBadge {
  private readonly aiService = inject(AiAnalysisService);

  readonly active = this.aiService.activeJob;
  readonly ready = this.aiService.readyJob;
  readonly failed = this.aiService.failedJob;

  readonly visible = computed(() => !!(this.active() || this.ready() || this.failed()));

  onClickReady() : void {
    const ready = this.ready();
    if(ready) {
      this.aiService.requestOpen(ready.id);
    }
  }

  dismissFailed() : void {
    const f = this.failed();
    if(f) this.aiService.consume(f.id);
  }

}
