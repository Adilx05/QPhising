import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-exports-table',
  templateUrl: './exports-table.component.html'
})
export class ExportsTableComponent {
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
