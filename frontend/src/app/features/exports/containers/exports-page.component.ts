import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-exports-page',
  templateUrl: './exports-page.component.html'
})
export class ExportsPageComponent {
  protected readonly exportJobs = [
    { report: 'Campaign Performance', format: 'Excel', status: 'Completed', requestedBy: 'analyst@corp.local' },
    { report: 'Risk Heatmap', format: 'PDF', status: 'Processing', requestedBy: 'operator@corp.local' },
    { report: 'Task Throughput', format: 'Excel', status: 'Queued', requestedBy: 'viewer@corp.local' }
  ];
}
