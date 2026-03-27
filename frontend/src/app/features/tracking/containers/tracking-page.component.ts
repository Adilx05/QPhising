import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-tracking-page',
  templateUrl: './tracking-page.component.html'
})
export class TrackingPageComponent {
  protected readonly events = [
    { campaign: 'Q1 Awareness', event: 'EmailOpen', risk: 'Low', occurredAt: '2026-03-27 09:15' },
    { campaign: 'Finance Drill', event: 'LinkClick', risk: 'Medium', occurredAt: '2026-03-27 09:31' },
    { campaign: 'Q1 Awareness', event: 'CredentialSubmit', risk: 'High', occurredAt: '2026-03-27 09:43' }
  ];
}
