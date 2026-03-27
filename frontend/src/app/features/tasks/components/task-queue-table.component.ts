import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-task-queue-table',
  templateUrl: './task-queue-table.component.html'
})
export class TaskQueueTableComponent {
  @Input({ required: true }) rows!: Array<Record<string, string>>;
}
