import { Component, computed, inject } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html'
})
export class DashboardPageComponent {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly kpis = this.appStateStore.dashboardKpis;
  protected readonly trend = this.appStateStore.dashboardTrend;
  protected readonly trendRows = this.appStateStore.dashboardTrendRows;
  protected readonly campaignRows = this.appStateStore.dashboardCampaigns;
  protected readonly viewState = computed(() => this.appStateStore.featureViewState().dashboard);
  protected readonly hasDashboardData = computed(
    () => this.kpis().length > 0 || this.trend().length > 0 || this.campaignRows().length > 0
  );

  protected readonly filters = ['Last 24 hours', 'Last 7 days', 'Last 30 days'];

  protected applyFilter(filter: string): void {
    this.appStateStore.updateFeatureFilter('dashboard', filter);
  }

  protected dismissError(): void {
    this.appStateStore.setFeatureError('dashboard', null);
  }
}
