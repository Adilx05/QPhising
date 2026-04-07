import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppApiFacade } from '../../../core/api/app-api.facade';
import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-exports-page',
  templateUrl: './exports-page.component.html'
})
export class ExportsPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);
  private readonly appApiFacade = inject(AppApiFacade);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().exports);
  protected readonly exportJobs = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    void this.loadExports();
  }

  private async loadExports(): Promise<void> {
    this.appStateStore.setFeatureLoading('exports', true);
    this.appStateStore.setFeatureError('exports', null);

    try {
      const exportJobIds = this.resolveTrackedExportJobIds();
      if (exportJobIds.length === 0) {
        this.exportJobs.set([]);
        return;
      }

      this.exportJobs.set(await this.appApiFacade.listExportRows(exportJobIds));
    } catch (error) {
      console.error('Failed to load exports', error);
      this.appStateStore.setFeatureError('exports', 'Export işleri backendden alınamadı.');
      this.exportJobs.set([]);
    } finally {
      this.appStateStore.setFeatureLoading('exports', false);
    }
  }

  private resolveTrackedExportJobIds(): string[] {
    if (typeof localStorage === 'undefined') {
      return [];
    }

    const serialized = localStorage.getItem('qphising_export_job_ids');
    if (!serialized) {
      return [];
    }

    try {
      const parsed = JSON.parse(serialized);
      if (!Array.isArray(parsed)) {
        return [];
      }

      return parsed.filter((id): id is string => typeof id === 'string' && id.length > 0);
    } catch {
      return [];
    }
  }
}
