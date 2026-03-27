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
  protected readonly trendRows = this.appStateStore.dashboardTrendRows;
  protected readonly viewState = computed(() => this.appStateStore.featureViewState().dashboard);
}
