import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-analytics-summary',
  templateUrl: './analytics-summary.component.html'
})
export class AnalyticsSummaryComponent {
  @Input({ required: true }) kpis!: Array<{ title: string; value: string }>;
  @Input({ required: true }) trendRows!: Array<Record<string, string>>;
}
