import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';

import { KpiCardComponent } from './components/kpi-card/kpi-card.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { EntityTableComponent } from './components/entity-table/entity-table.component';

@NgModule({
  declarations: [KpiCardComponent, PageHeaderComponent, EntityTableComponent],
  imports: [CommonModule, CardModule, TableModule],
  exports: [KpiCardComponent, PageHeaderComponent, EntityTableComponent, CardModule, TableModule]
})
export class SharedModule {}
