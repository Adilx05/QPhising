import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-campaigns-table',
  templateUrl: './campaigns-table.component.html'
})
export class CampaignsTableComponent {
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
