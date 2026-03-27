import { Component, computed, inject } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-campaigns-page',
  templateUrl: './campaigns-page.component.html'
})
export class CampaignsPageComponent {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().campaigns);
  protected readonly campaigns = [
    { name: 'Q1 Awareness', template: 'CredentialHarvest', status: 'Active', owner: 'Ops Team' },
    { name: 'Finance Drill', template: 'InvoiceFraud', status: 'Scheduled', owner: 'Risk Team' },
    { name: 'HR Notice', template: 'PasswordReset', status: 'Draft', owner: 'People Ops' }
  ];
}
