import { Component, computed, inject } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-exports-page',
  templateUrl: './exports-page.component.html'
})
export class ExportsPageComponent {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().exports);
  protected readonly exportJobs = [
    { report: 'Campaign Performance', format: 'Excel', status: 'Completed', requestedBy: 'analyst@corp.local' },
    { report: 'Risk Heatmap', format: 'PDF', status: 'Processing', requestedBy: 'operator@corp.local' },
    { report: 'Task Throughput', format: 'Excel', status: 'Queued', requestedBy: 'viewer@corp.local' }
  ];
}
