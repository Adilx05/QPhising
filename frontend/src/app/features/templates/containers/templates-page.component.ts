import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppApiFacade } from '../../../core/api/app-api.facade';
import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-templates-page',
  templateUrl: './templates-page.component.html'
})
export class TemplatesPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);
  private readonly appApiFacade = inject(AppApiFacade);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().templates);
  protected readonly templates = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    void this.loadTemplates();
  }

  private async loadTemplates(): Promise<void> {
    this.appStateStore.setFeatureLoading('templates', true);
    this.appStateStore.setFeatureError('templates', null);

    try {
      this.templates.set(await this.appApiFacade.listTemplateRows());
    } catch (error) {
      console.error('Failed to load templates', error);
      this.appStateStore.setFeatureError('templates', 'Template listesi backendden alınamadı.');
      this.templates.set([]);
    } finally {
      this.appStateStore.setFeatureLoading('templates', false);
    }
  }
}
