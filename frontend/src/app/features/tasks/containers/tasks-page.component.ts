import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-tasks-page',
  templateUrl: './tasks-page.component.html'
})
export class TasksPageComponent {
  protected readonly tasks = [
    { type: 'EmailDispatch', status: 'Processing', attempts: '1', queuedAt: '2026-03-27 09:50' },
    { type: 'ClickEventProcessing', status: 'Queued', attempts: '0', queuedAt: '2026-03-27 09:53' },
    { type: 'ExportGeneration', status: 'Completed', attempts: '1', queuedAt: '2026-03-27 09:01' }
  ];
}
