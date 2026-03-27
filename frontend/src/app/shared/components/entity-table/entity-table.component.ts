import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-entity-table',
  templateUrl: './entity-table.component.html'
})
export class EntityTableComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
