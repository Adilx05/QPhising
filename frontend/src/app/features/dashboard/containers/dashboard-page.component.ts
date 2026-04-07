import { Component, OnInit, computed, inject } from '@angular/core';

import { AppApiFacade } from '../../../core/api/app-api.facade';
import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html'
})
export class DashboardPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);
  private readonly appApiFacade = inject(AppApiFacade);

  protected readonly kpis = this.appStateStore.dashboardKpis;
  protected readonly trend = this.appStateStore.dashboardTrend;
  protected readonly trendRows = this.appStateStore.dashboardTrendRows;
  protected readonly campaignRows = this.appStateStore.dashboardCampaigns;
  protected readonly viewState = computed(() => this.appStateStore.featureViewState().dashboard);
  protected readonly hasDashboardData = computed(
    () => this.kpis().length > 0 || this.trend().length > 0 || this.campaignRows().length > 0
  );

  protected readonly filters = ['Last 24 hours', 'Last 7 days', 'Last 30 days'];

  ngOnInit(): void {
    void this.loadDashboardData(this.viewState().activeFilter);
  }

  protected applyFilter(filter: string): void {
    this.appStateStore.updateFeatureFilter('dashboard', filter);
    void this.loadDashboardData(filter);
  }

  protected dismissError(): void {
    this.appStateStore.setFeatureError('dashboard', null);
  }

  private async loadDashboardData(filter: string): Promise<void> {
    this.appStateStore.setFeatureLoading('dashboard', true);
    this.appStateStore.setFeatureError('dashboard', null);

    try {
      const rangeInDays = filter === 'Last 24 hours' ? 1 : filter === 'Last 30 days' ? 30 : 7;
      const dashboard = await this.appApiFacade.getDashboardData(rangeInDays);

      this.appStateStore.setDashboardData({
        kpis: dashboard.kpis,
        trend: dashboard.trendRows,
        campaigns: dashboard.campaigns
      });
    } catch (error) {
      console.error('Failed to load dashboard data', error);
      this.appStateStore.setFeatureError('dashboard', 'Dashboard verisi backendden alınamadı. Token ve gateway adresini kontrol et.');
      this.appStateStore.setDashboardData({ kpis: [], trend: [], campaigns: [] });
    } finally {
      this.appStateStore.setFeatureLoading('dashboard', false);
    }
  }
}
