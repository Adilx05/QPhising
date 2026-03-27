import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-tracking-events-table',
  templateUrl: './tracking-events-table.component.html'
})
export class TrackingEventsTableComponent {
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
