import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-analytics-page',
  templateUrl: './analytics-page.component.html'
})
export class AnalyticsPageComponent {
  protected readonly kpis = [
    { title: 'Open Rate', value: '72.6%' },
    { title: 'Click Rate', value: '18.3%' },
    { title: 'Credential Submit', value: '6.1%' }
  ];

  protected readonly trendRows = [
    { period: 'Week 1', clickRate: '14%' },
    { period: 'Week 2', clickRate: '16%' },
    { period: 'Week 3', clickRate: '19%' },
    { period: 'Week 4', clickRate: '18%' }
  ];
}
