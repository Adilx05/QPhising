import { Component, Input } from '@angular/core';
import { Table } from 'primeng/table';

import { DashboardCampaignRow } from '../../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-dashboard-campaigns-table',
  templateUrl: './dashboard-campaigns-table.component.html'
})
export class DashboardCampaignsTableComponent {
  @Input({ required: true }) rows!: DashboardCampaignRow[];

  protected readonly statuses: DashboardCampaignRow['status'][] = ['Draft', 'Scheduled', 'Active', 'Ended'];

  protected statusSeverity(status: DashboardCampaignRow['status']): 'success' | 'warn' | 'info' | 'secondary' {
    if (status === 'Active') {
      return 'success';
    }

    if (status === 'Scheduled') {
      return 'info';
    }

    if (status === 'Draft') {
      return 'warn';
    }

    return 'secondary';
  }

  protected applyGlobalFilter(event: Event, table: Table): void {
    const inputElement = event.target as HTMLInputElement | null;
    table.filterGlobal(inputElement?.value ?? '', 'contains');
  }

  protected applyStatusFilter(status: DashboardCampaignRow['status'] | null, table: Table): void {
    table.filter(status, 'status', 'equals');
  }
}
