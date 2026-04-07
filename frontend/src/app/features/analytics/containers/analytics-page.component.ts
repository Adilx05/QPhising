import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppApiFacade } from '../../../core/api/app-api.facade';
import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-analytics-page',
  templateUrl: './analytics-page.component.html'
})
export class AnalyticsPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);
  private readonly appApiFacade = inject(AppApiFacade);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().analytics);
  protected readonly kpis = signal<Array<{ title: string; value: string }>>([]);
  protected readonly trendRows = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    void this.loadAnalytics();
  }

  private async loadAnalytics(): Promise<void> {
    this.appStateStore.setFeatureLoading('analytics', true);
    this.appStateStore.setFeatureError('analytics', null);

    try {
      const data = await this.appApiFacade.getAnalyticsSummary(30);
      this.kpis.set(data.kpis);
      this.trendRows.set(data.trendRows);
    } catch (error) {
      console.error('Failed to load analytics summary', error);
      this.appStateStore.setFeatureError('analytics', 'Analytics verisi backendden alınamadı.');
      this.kpis.set([]);
      this.trendRows.set([]);
    } finally {
      this.appStateStore.setFeatureLoading('analytics', false);
    }
  }
}
