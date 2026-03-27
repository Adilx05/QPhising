import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../../shared/shared.module';
import { AnalyticsRoutingModule } from './analytics-routing.module';
import { AnalyticsPageComponent } from './containers/analytics-page.component';
import { AnalyticsSummaryComponent } from './components/analytics-summary.component';

@NgModule({
  declarations: [AnalyticsPageComponent, AnalyticsSummaryComponent],
  imports: [CommonModule, SharedModule, AnalyticsRoutingModule]
})
export class AnalyticsModule {}
