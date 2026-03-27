import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html'
})
export class DashboardPageComponent {
  protected readonly trendRows = [
    { day: 'Mon', clicks: '120' },
    { day: 'Tue', clicks: '145' },
    { day: 'Wed', clicks: '98' },
    { day: 'Thu', clicks: '173' },
    { day: 'Fri', clicks: '160' }
  ];
}
