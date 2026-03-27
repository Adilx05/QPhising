import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html'
})
export class DashboardPageComponent {
  protected readonly chartData = {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
    datasets: [
      {
        label: 'Phishing Clicks',
        data: [120, 145, 98, 173, 160],
        fill: false,
        borderColor: '#0ea5e9',
        tension: 0.4
      }
    ]
  };

  protected readonly chartOptions = {
    plugins: {
      legend: {
        labels: {
          color: '#334155'
        }
      }
    },
    scales: {
      x: {
        ticks: {
          color: '#64748b'
        }
      },
      y: {
        ticks: {
          color: '#64748b'
        }
      }
    }
  };
}
