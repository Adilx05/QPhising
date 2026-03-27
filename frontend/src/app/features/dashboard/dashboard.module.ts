import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardPageComponent } from './containers/dashboard-page.component';
import { SharedModule } from '../../shared/shared.module';
import { DashboardTrendChartComponent } from './components/dashboard-trend-chart/dashboard-trend-chart.component';
import { DashboardCampaignsTableComponent } from './components/dashboard-campaigns-table/dashboard-campaigns-table.component';

@NgModule({
  declarations: [DashboardPageComponent, DashboardTrendChartComponent, DashboardCampaignsTableComponent],
  imports: [CommonModule, SharedModule, DashboardRoutingModule, InputTextModule, DropdownModule, TagModule]
})
export class DashboardModule {}
