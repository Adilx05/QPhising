import { Component, computed, inject } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-templates-page',
  templateUrl: './templates-page.component.html'
})
export class TemplatesPageComponent {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().templates);
  protected readonly templates = [
    { name: 'MFA Expiration', type: 'CredentialHarvest', quality: 'Approved', owner: 'Content Team' },
    { name: 'Executive Invoice', type: 'InvoiceFraud', quality: 'In Review', owner: 'Threat Team' },
    { name: 'VPN Reset', type: 'PasswordReset', quality: 'Approved', owner: 'Ops Team' }
  ];
}
