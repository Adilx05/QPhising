import { Component, Input } from '@angular/core';

import { DashboardTrendPoint } from '../../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-dashboard-trend-chart',
  templateUrl: './dashboard-trend-chart.component.html'
})
export class DashboardTrendChartComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) points!: DashboardTrendPoint[];

  protected maxClicks(): number {
    const max = this.points.reduce((currentMax, point) => Math.max(currentMax, point.clicks), 0);
    return Math.max(max, 1);
  }
}
