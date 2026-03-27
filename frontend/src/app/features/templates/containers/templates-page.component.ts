import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-templates-page',
  templateUrl: './templates-page.component.html'
})
export class TemplatesPageComponent {
  protected readonly templates = [
    { name: 'MFA Expiration', type: 'CredentialHarvest', quality: 'Approved', owner: 'Content Team' },
    { name: 'Executive Invoice', type: 'InvoiceFraud', quality: 'In Review', owner: 'Threat Team' },
    { name: 'VPN Reset', type: 'PasswordReset', quality: 'Approved', owner: 'Ops Team' }
  ];
}
