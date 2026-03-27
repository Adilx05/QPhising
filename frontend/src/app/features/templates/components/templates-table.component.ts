import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-templates-table',
  templateUrl: './templates-table.component.html'
})
export class TemplatesTableComponent {
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
