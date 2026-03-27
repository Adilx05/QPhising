import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-campaigns-page',
  templateUrl: './campaigns-page.component.html'
})
export class CampaignsPageComponent {
  protected readonly campaigns = [
    { name: 'Q1 Awareness', template: 'CredentialHarvest', status: 'Active', owner: 'Ops Team' },
    { name: 'Finance Drill', template: 'InvoiceFraud', status: 'Scheduled', owner: 'Risk Team' },
    { name: 'HR Notice', template: 'PasswordReset', status: 'Draft', owner: 'People Ops' }
  ];
}
