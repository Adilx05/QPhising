import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardPageComponent } from './containers/dashboard-page.component';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';

@NgModule({
  declarations: [DashboardPageComponent, KpiCardComponent],
  imports: [CommonModule, DashboardRoutingModule, CardModule, ChartModule]
})
export class DashboardModule {}
